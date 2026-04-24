using System;
using System.IO;
using UnityEngine;

namespace APEX.Meta
{
    /// <summary>
    /// Serializable settings payload. Persisted as JSON under
    /// Application.persistentDataPath/settings.json. Fields are read/written by
    /// SettingsService and applied to Unity subsystems from there.
    /// </summary>
    [Serializable]
    public class SettingsStore
    {
        public float masterVolume = 1f;
        public bool fullscreen = true;
        public bool vsync = true;
        public bool autoSelectGenericPicks = false;

        private const string FileName = "settings.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static SettingsStore Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new SettingsStore();
                var json = File.ReadAllText(FilePath);
                var store = JsonUtility.FromJson<SettingsStore>(json);
                return store ?? new SettingsStore();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[APEX] Settings load failed, using defaults: {e.Message}");
                return new SettingsStore();
            }
        }

        public static void Save(SettingsStore store)
        {
            try
            {
                var json = JsonUtility.ToJson(store, prettyPrint: true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[APEX] Settings save failed: {e.Message}");
            }
        }
    }
}
