using System;
using System.Collections.Generic;
using APEX.Tags;
using UnityEngine;

namespace APEX.Player
{
    /// <summary>
    /// Authored stat block for the player. Consumed by PlayerController, PlayerCombat,
    /// and PassiveMeleeStream at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Player_Stats", menuName = "APEX/Player/Player Stats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Vitals")]
        public float maxHp = 60f;
        public float moveSpeed = 6f;

        [Header("Passive Melee Stream")]
        public bool passiveStreamEnabled = true;
        public float passiveBiteRadius = 1.2f;
        public float passiveBiteIntervalSeconds = 0.25f;
        public float passiveBiteDamage = 3f;
        public TagSet passiveBiteTags;

        [Header("Damage Multipliers (by tag)")]
        [SerializeField] private List<TagMultiplier> _damageMultipliers = new();

        /// <summary>
        /// Returns the product of all multipliers whose tag appears in the attack's tag set.
        /// Default 1.0 if no matches.
        /// </summary>
        public float GetMultiplierFor(TagSet attackTags)
        {
            float result = 1f;
            for (int i = 0; i < _damageMultipliers.Count; i++)
            {
                var entry = _damageMultipliers[i];
                if (entry.tag == null) continue;
                if (attackTags.Contains(entry.tag))
                {
                    result *= entry.multiplier;
                }
            }
            return result;
        }

        [Serializable]
        public struct TagMultiplier
        {
            public TagDefinition tag;
            public float multiplier;
        }
    }
}
