using System;
using System.Collections.Generic;
using System.IO;
using APEX.Core;
using APEX.Core.Events;
using UnityEngine;

namespace APEX.Meta
{
    /// <summary>
    /// Persistent singleton that appends a RunRecord to stats.json whenever a run ends.
    /// Lives on the APEX_PersistentServices root alongside SettingsService. Consumers
    /// (e.g. StatsScreen) read the full history via <see cref="GetAll"/>.
    /// </summary>
    public class StatsRecorder : MonoBehaviour
    {
        public static StatsRecorder Instance { get; private set; }

        private const string FileName = "stats.json";
        public const int SchemaVersion = 1;

        private StatsFile _file;

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public IReadOnlyList<RunRecord> GetAll()
        {
            EnsureLoaded();
            return _file.records;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            EnsureLoaded();
        }

        private void OnEnable() { EventBus.OnRunEnded += OnRunEnded; }
        private void OnDisable() { EventBus.OnRunEnded -= OnRunEnded; }

        private void OnRunEnded(RunStats stats, EndReason reason)
        {
            EnsureLoaded();
            _file.records.Add(new RunRecord
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                endReason = reason.ToString(),
                stats = stats
            });
            Save();
        }

        private void EnsureLoaded()
        {
            if (_file != null) return;
            _file = Load();
        }

        private static StatsFile Load()
        {
            if (!File.Exists(FilePath)) return NewFile();

            string json;
            try
            {
                json = File.ReadAllText(FilePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[APEX] stats.json read failed, starting fresh: {e.Message}");
                return NewFile();
            }

            StatsFile parsed;
            try
            {
                parsed = JsonUtility.FromJson<StatsFile>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[APEX] stats.json parse failed, quarantining: {e.Message}");
                Quarantine();
                return NewFile();
            }

            // JsonUtility.FromJson is permissive: it returns a default-constructed object for
            // many malformed inputs rather than throwing. Validate the shape explicitly so
            // silent corruption (e.g. missing version, truncated structure) still quarantines.
            if (parsed == null || parsed.version != SchemaVersion || parsed.records == null)
            {
                Debug.LogWarning(
                    "[APEX] stats.json failed validation after parse — version or records missing. Quarantining.");
                Quarantine();
                return NewFile();
            }

            return parsed;
        }

        private void Save()
        {
            try
            {
                var json = JsonUtility.ToJson(_file, prettyPrint: true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[APEX] stats.json write failed: {e.Message}");
            }
        }

        private static void Quarantine()
        {
            try
            {
                string ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                string basePath = FilePath + ".corrupt." + ts;
                string corruptPath = basePath;
                if (File.Exists(corruptPath))
                {
                    bool resolved = false;
                    for (int i = 1; i <= 10; i++)
                    {
                        corruptPath = basePath + "." + i;
                        if (!File.Exists(corruptPath))
                        {
                            resolved = true;
                            break;
                        }
                    }
                    if (!resolved)
                    {
                        Debug.LogError(
                            $"[APEX] stats.json quarantine failed: exhausted 10 collision suffixes for {basePath}. Leaving original in place.");
                        return;
                    }
                }
                File.Move(FilePath, corruptPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[APEX] stats.json quarantine failed: {e.Message}");
            }
        }

        private static StatsFile NewFile() => new() { version = SchemaVersion, records = new List<RunRecord>() };

        [Serializable]
        public class StatsFile
        {
            public int version;
            public List<RunRecord> records;
        }
    }

    /// <summary>One recorded run. Serialized inside stats.json.</summary>
    [Serializable]
    public class RunRecord
    {
        public string timestamp;   // ISO 8601 UTC
        public string endReason;   // "Victory" | "Defeat" | "Quit"
        public RunStats stats;
    }
}
