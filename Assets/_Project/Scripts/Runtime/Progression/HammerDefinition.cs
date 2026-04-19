using APEX.Combat.Attacks;
using APEX.Tags;
using UnityEngine;

namespace APEX.Progression
{
    public enum HammerKind
    {
        IgniteLunge,
        TwinSpit,
        RingingPulse,
        StickyField,
        HarpoonLunge,
        PressurizedSpit,
        GravityPulse,
        InfectingField
    }

    /// <summary>
    /// Authored hammer pick. Hammers mutate exactly one core attack. The target attack
    /// defines which slot (Lunge/Spit/Pulse/Field) the hammer slots into.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Hammer_New", menuName = "APEX/Progression/Hammer")]
    public class HammerDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Rarity rarity;
        public AttackDefinition targetAttack;
        public TagSet tagsAdded;
        public HammerKind kind;
    }
}
