using System.Collections.Generic;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Rolls pick options for the level-up screen. Respects the pick table's band percentages
    /// and the player's build dedup (no maxed passives, no already-applied hammers).
    /// </summary>
    public static class PickRoller
    {
        public struct PickOption
        {
            public PassiveDefinition passive; // null if hammer
            public HammerDefinition hammer;   // null if passive
            public bool IsHammer => hammer != null;
            public Rarity Rarity => hammer != null ? hammer.rarity : passive.rarity;
            public string DisplayName => hammer != null ? hammer.displayName : passive.displayName;
            public string Description => hammer != null ? hammer.description : passive.description;
        }

        public static List<PickOption> RollPicks(
            int level, int count, PickTable table, PickPool pool, PlayerBuild build)
        {
            var result = new List<PickOption>(count);
            if (table == null || pool == null) return result;

            var band = table.GetBandFor(level);

            var usedPassives = new HashSet<PassiveDefinition>();
            var usedHammers = new HashSet<HammerDefinition>();

            for (int i = 0; i < count; i++)
            {
                bool isHammer = Random.value < band.hammerPct;
                var rarity = RollRarity(band);

                PickOption? opt = isHammer
                    ? TryRollHammer(pool, build, rarity, usedHammers)
                    : TryRollPassive(pool, build, rarity, usedPassives);

                // Fallback 1: flip type if the preferred side has nothing matching.
                if (opt == null)
                {
                    opt = isHammer
                        ? TryRollPassive(pool, build, rarity, usedPassives)
                        : TryRollHammer(pool, build, rarity, usedHammers);
                }

                // Fallback 2: ignore rarity constraint.
                if (opt == null)
                {
                    opt = TryRollPassive(pool, build, null, usedPassives)
                        ?? TryRollHammer(pool, build, null, usedHammers);
                }

                if (opt != null) result.Add(opt.Value);
            }

            return result;
        }

        private static Rarity RollRarity(PickTable.LevelBand band)
        {
            float r = Random.value;
            if (r < band.commonPct) return Rarity.Common;
            if (r < band.commonPct + band.rarePct) return Rarity.Rare;
            return Rarity.Epic;
        }

        private static PickOption? TryRollPassive(
            PickPool pool, PlayerBuild build, Rarity? rarity, HashSet<PassiveDefinition> used)
        {
            var eligible = new List<PassiveDefinition>();
            for (int i = 0; i < pool.passives.Count; i++)
            {
                var p = pool.passives[i];
                if (p == null) continue;
                if (used.Contains(p)) continue;
                if (!build.CanPick(p)) continue;
                if (rarity.HasValue && p.rarity != rarity.Value) continue;
                eligible.Add(p);
            }
            if (eligible.Count == 0) return null;
            var pick = eligible[Random.Range(0, eligible.Count)];
            used.Add(pick);
            return new PickOption { passive = pick };
        }

        private static PickOption? TryRollHammer(
            PickPool pool, PlayerBuild build, Rarity? rarity, HashSet<HammerDefinition> used)
        {
            var eligible = new List<HammerDefinition>();
            for (int i = 0; i < pool.hammers.Count; i++)
            {
                var h = pool.hammers[i];
                if (h == null) continue;
                if (used.Contains(h)) continue;
                if (!build.CanPick(h)) continue;
                if (rarity.HasValue && h.rarity != rarity.Value) continue;
                eligible.Add(h);
            }
            if (eligible.Count == 0) return null;
            var pick = eligible[Random.Range(0, eligible.Count)];
            used.Add(pick);
            return new PickOption { hammer = pick };
        }
    }
}
