using APEX.Player;
using APEX.Tags;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Authored data for an attack slot. Concrete subclasses add attack-specific tuning
    /// fields and construct the matching IAttackInstance at runtime.
    /// </summary>
    public abstract class AttackDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public float baseCooldownSeconds = 1f;
        public float baseDamage = 10f;
        public TagSet tags;
        public GameObject vfxPrefab;
        public bool autoFireByDefault = true;

        public abstract IAttackInstance CreateInstance(PlayerCombat combat);
    }
}
