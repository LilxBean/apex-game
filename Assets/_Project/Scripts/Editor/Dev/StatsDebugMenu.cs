#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using APEX.Core;
using APEX.Meta;
using UnityEditor;
using UnityEngine;

namespace APEX.Editor.Dev
{
    /// <summary>
    /// Developer-only menu items for exercising StatsRecorder's persistence paths:
    /// seeding a synthetic history, corrupting stats.json to verify Load() quarantines,
    /// and appending a prior-year record so StatsScreen date formatting can be verified.
    /// Reuses StatsRecorder.StatsFile / RunRecord so the on-disk schema stays in lockstep.
    /// </summary>
    public static class StatsDebugMenu
    {
        private static string FilePath => StatsRecorder.FilePath;

        [MenuItem("APEX/Dev/Stats/Seed 25 Synthetic Records")]
        public static void Seed25()
        {
            if (File.Exists(FilePath))
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Overwrite stats.json?",
                    $"{FilePath}\n\nThis will replace the existing stats.json with 25 synthetic records.",
                    "Overwrite", "Cancel");
                if (!proceed) return;
            }

            var file = new StatsRecorder.StatsFile
            {
                version = StatsRecorder.SchemaVersion,
                records = BuildSyntheticRecords(25)
            };

            WriteFile(file);

            EditorUtility.DisplayDialog(
                "Seeded stats.json",
                $"25 synthetic records written to:\n{FilePath}",
                "OK");
        }

        [MenuItem("APEX/Dev/Stats/Corrupt stats.json")]
        public static void CorruptStatsJson()
        {
            if (!File.Exists(FilePath))
            {
                EditorUtility.DisplayDialog(
                    "No stats.json",
                    $"Nothing to corrupt — {FilePath} does not exist.\nRun 'Seed 25 Synthetic Records' first.",
                    "OK");
                return;
            }

            byte[] bytes = File.ReadAllBytes(FilePath);
            if (bytes.Length < 2)
            {
                EditorUtility.DisplayDialog(
                    "stats.json too short",
                    "File has fewer than 2 bytes; nothing to reliably corrupt.",
                    "OK");
                return;
            }

            // Truncate to the first 20% of byte length. JsonUtility cannot recover anything
            // useful once the tail (including closing braces and most record data) is gone.
            int keep = Mathf.Max(1, bytes.Length / 5);
            var truncated = new byte[keep];
            Buffer.BlockCopy(bytes, 0, truncated, 0, keep);
            File.WriteAllBytes(FilePath, truncated);

            EditorUtility.DisplayDialog(
                "Corrupted stats.json",
                $"Truncated {FilePath} to {keep} / {bytes.Length} bytes (~20%).\n\nStatsRecorder.Load() should now quarantine this file and start fresh.",
                "OK");
        }

        [MenuItem("APEX/Dev/Stats/Add Prior-Year Record")]
        public static void AddPriorYearRecord()
        {
            if (!File.Exists(FilePath))
            {
                EditorUtility.DisplayDialog(
                    "No stats.json",
                    $"Cannot append — {FilePath} does not exist.\nRun 'Seed 25 Synthetic Records' first.",
                    "OK");
                return;
            }

            StatsRecorder.StatsFile file;
            try
            {
                string json = File.ReadAllText(FilePath);
                file = JsonUtility.FromJson<StatsRecorder.StatsFile>(json);
                if (file == null) throw new Exception("null after parse");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog(
                    "stats.json unreadable",
                    $"Could not parse {FilePath}:\n{e.Message}\n\nRe-seed first.",
                    "OK");
                return;
            }

            file.records ??= new List<RunRecord>();

            var priorYear = DateTime.UtcNow.AddYears(-1).AddDays(-5);
            file.records.Add(new RunRecord
            {
                timestamp = priorYear.ToString("o"),
                endReason = EndReason.Victory.ToString(),
                stats = new RunStats
                {
                    RunDurationSeconds = 275f,
                    LevelReached = 12,
                    Kills = 1180,
                    DamageDealt = 42500f,
                    PeakDPS = 310f,
                    XPTotal = 2400,
                    PicksTaken = 11,
                    PassivesTaken = 8,
                    HammersTaken = 3,
                }
            });

            WriteFile(file);

            EditorUtility.DisplayDialog(
                "Added prior-year record",
                $"Appended 1 record dated {priorYear.ToLocalTime():yyyy-MM-dd HH:mm} to:\n{FilePath}",
                "OK");
        }

        // --- helpers ----------------------------------------------------------

        private static List<RunRecord> BuildSyntheticRecords(int count)
        {
            // Deterministic seed so repeated "seed" clicks produce comparable data.
            var rng = new System.Random(1337);
            var records = new List<RunRecord>(count);

            // 10 Victory / 8 Defeat / 7 Quit.
            var reasons = new List<EndReason>(count);
            for (int i = 0; i < 10; i++) reasons.Add(EndReason.Victory);
            for (int i = 0; i < 8; i++) reasons.Add(EndReason.Defeat);
            for (int i = 0; i < 7; i++) reasons.Add(EndReason.Quit);
            Shuffle(reasons, rng);

            var nowUtc = DateTime.UtcNow;
            for (int i = 0; i < count; i++)
            {
                // Spread timestamps across the last 30 days, in random order (not monotonic).
                double hoursAgo = rng.NextDouble() * 30.0 * 24.0;
                var ts = nowUtc.AddHours(-hoursAgo);

                var reason = reasons[i];

                float duration = reason == EndReason.Victory
                    ? (float)(240.0 + rng.NextDouble() * 60.0)   // 240-300 for wins
                    : (float)(60.0 + rng.NextDouble() * 210.0);  // 60-270 otherwise

                int level = 3 + rng.Next(13); // 3..15
                int kills = 100 + rng.Next(1901); // 100..2000
                float damage = 5000f + (float)(rng.NextDouble() * 80000.0);
                float peakDps = 80f + (float)(rng.NextDouble() * 520.0);
                int xp = 200 + rng.Next(4800);
                int picks = rng.Next(2, 16);
                int passives = rng.Next(0, picks + 1);
                int hammers = picks - passives;

                records.Add(new RunRecord
                {
                    timestamp = ts.ToString("o"),
                    endReason = reason.ToString(),
                    stats = new RunStats
                    {
                        RunDurationSeconds = duration,
                        LevelReached = level,
                        Kills = kills,
                        DamageDealt = damage,
                        PeakDPS = peakDps,
                        XPTotal = xp,
                        PicksTaken = picks,
                        PassivesTaken = passives,
                        HammersTaken = hammers,
                    }
                });
            }

            return records;
        }

        private static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static void WriteFile(StatsRecorder.StatsFile file)
        {
            string dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonUtility.ToJson(file, prettyPrint: true));
        }
    }
}
#endif
