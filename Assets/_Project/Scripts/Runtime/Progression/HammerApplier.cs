using APEX.Combat.Attacks;
using APEX.Tags;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Applies runtime mutations for a HammerDefinition. Adds tags to the target attack
    /// instance, sets mutation flags, and logs for stub effects.
    /// </summary>
    public static class HammerApplier
    {
        public static void Apply(HammerDefinition hammer, IAttackInstance target, PlayerBuild build)
        {
            if (target is AttackInstanceBase baseInstance)
            {
                baseInstance.AppliedHammerIds.Add(hammer.id);
                foreach (var tag in hammer.tagsAdded.Tags)
                {
                    if (tag != null) baseInstance.Mutations.ExtraTags.Add(tag);
                }

                switch (hammer.kind)
                {
                    case HammerKind.IgniteLunge:
                        // Tags [Fire][Burn] already added; burn DoT stub.
                        break;
                    case HammerKind.TwinSpit:
                        baseInstance.Mutations.ProjectileCountBonus += 1;
                        break;
                    case HammerKind.PressurizedSpit:
                        baseInstance.Mutations.PierceCount += 3;
                        break;
                    case HammerKind.HarpoonLunge:
                        baseInstance.Mutations.ChainCount += 2;
                        break;
                    case HammerKind.RingingPulse:
                    case HammerKind.StickyField:
                    case HammerKind.GravityPulse:
                    case HammerKind.InfectingField:
                        // Tag-only stubs; concrete effect deferred.
                        break;
                }
            }

            Debug.Log($"[Hammer] Applied '{hammer.displayName}' to {hammer.targetAttack?.displayName}.");
        }
    }
}
