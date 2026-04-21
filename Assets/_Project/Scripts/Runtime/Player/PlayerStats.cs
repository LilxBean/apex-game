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

        [Header("Damage Multipliers")]
        [Tooltip("Global damage multiplier applied to every attack regardless of tags.")]
        public float globalDamageMultiplier = 1f;

        [Header("Damage Multipliers (by tag)")]
        [SerializeField] private List<TagMultiplier> _damageMultipliers = new();

        /// <summary>
        /// Returns the product of all multipliers whose tag appears in the attack's tag set.
        /// Default 1.0 if no matches.
        /// </summary>
        public float GetMultiplierFor(TagSet attackTags)
        {
            float result = globalDamageMultiplier;
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

        /// <summary>
        /// Adds a multiplier that applies whenever an attack carries any of the passive's tags.
        /// Runtime-only — callers should ensure they're mutating a runtime clone, not the asset.
        /// </summary>
        public void AddDamageMultiplier(TagSet filterTags, float multiplier)
        {
            foreach (var tag in filterTags.Tags)
            {
                if (tag == null) continue;
                _damageMultipliers.Add(new TagMultiplier { tag = tag, multiplier = multiplier });
            }
        }

        [Serializable]
        public struct TagMultiplier
        {
            public TagDefinition tag;
            public float multiplier;
        }
    }
}
