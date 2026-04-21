using APEX.Tags;
using UnityEngine;

namespace APEX.Progression
{
    public enum PassiveKind
    {
        Ravenous,
        SharperTeeth,
        Swift,
        QuickBiter,
        AcidSpray,
        Corrosion,
        PulseOverlap,
        FieldLingering,
        ApexHunger,
        SwarmRend,
        Voidburst,
        ConsumingWave,
        Endurance,
        Ferocity
    }

    /// <summary>
    /// Authored passive pick. One SO per kind; the kind enum drives the runtime effect
    /// applied through <see cref="PassiveApplier"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Passive_New", menuName = "APEX/Progression/Passive")]
    public class PassiveDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Rarity rarity;
        public TagSet tags;
        public PassiveKind kind;
        public int maxStacks = 1;

        /// <summary>Fraction used by magnitude-driven kinds (Endurance, Ferocity). 0.05 = +5% per stack.</summary>
        public float magnitude = 0f;

        /// <summary>Generic fallback picks are excluded from the normal roll pool and only appear when the real pool is exhausted.</summary>
        public bool IsGeneric = false;
    }
}
