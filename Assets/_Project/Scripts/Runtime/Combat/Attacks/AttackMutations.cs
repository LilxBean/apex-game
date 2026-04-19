using System.Collections.Generic;
using APEX.Tags;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Per-instance tunables that hammers and passives mutate at runtime, leaving the shared
    /// AttackDefinition asset untouched. Attack instances read these when firing.
    /// </summary>
    public class AttackMutations
    {
        public float CooldownMultiplier = 1f;
        public float DamageMultiplier = 1f;
        public float RadiusMultiplier = 1f;
        public float DurationMultiplier = 1f;
        public int ProjectileCountBonus;
        public int PierceCount;
        public int ChainCount;

        public readonly List<TagDefinition> ExtraTags = new();

        /// <summary>Combines the attack definition tags with extra tags added by hammers.</summary>
        public TagSet BuildTags(TagSet baseTags)
        {
            if (ExtraTags.Count == 0) return baseTags;

            var merged = new List<TagDefinition>();
            var src = baseTags.Tags;
            for (int i = 0; i < src.Count; i++) merged.Add(src[i]);
            for (int i = 0; i < ExtraTags.Count; i++)
            {
                var t = ExtraTags[i];
                if (t != null && !merged.Contains(t)) merged.Add(t);
            }
            return new TagSet(merged);
        }
    }
}
