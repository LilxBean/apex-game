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

        // Per-run, alternates HP/ATK across generic slots (even → HP, odd → ATK).
        // Increments per generic SLOT FILLED, not per selection. Reset at run start.
        private static int _genericCounter;

        public static void ResetForNewRun()
        {
            _genericCounter = 0;
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
                else break; // real pool exhausted — remaining slots filled with generics below.
            }

            // Fill any remaining slots with alternating generic HP/ATK picks.
            int genericsUsed = 0;
            while (result.Count < count)
            {
                bool wantHp = (_genericCounter % 2 == 0);
                var generic = FindGeneric(pool, wantHp ? PassiveKind.Endurance : PassiveKind.Ferocity);
                if (generic == null) break; // generic asset missing — nothing else to do.

                result.Add(new PickOption { passive = generic });
                _genericCounter++;
                genericsUsed++;
            }

            if (genericsUsed > 0)
            {
                Debug.Log($"[APEX] Pick pool exhausted at level {level}, filled {genericsUsed} slots with generics.");
            }

            return result;
        }

        private static PassiveDefinition FindGeneric(PickPool pool, PassiveKind kind)
        {
            for (int i = 0; i < pool.passives.Count; i++)
            {
                var p = pool.passives[i];
                if (p == null) continue;
                if (!p.IsGeneric) continue;
                if (p.kind == kind) return p;
            }
            return null;
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
                if (p.IsGeneric) continue; // generics only appear as fallback, never in the base roll.
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
