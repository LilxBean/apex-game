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
        ConsumingWave
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
    }
}
