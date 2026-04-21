using APEX.Combat;
using APEX.Combat.Attacks;
using APEX.Core.Events;
using APEX.Enemies;
using APEX.Tags;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Applies runtime effects for a PassiveDefinition. Effects read from the passive's kind.
    /// Subscribes to EventBus for on-hit/on-kill/on-level-up passives.
    /// </summary>
    public static class PassiveApplier
    {
        public static void Apply(PassiveDefinition passive, PlayerBuild build)
        {
            var stats = build.Controller.Stats;
            switch (passive.kind)
            {
                case PassiveKind.Ravenous:
                    EventBus.OnEnemyKilled += (enemy, dmg) =>
                    {
                        if (build == null) return;
                        var hp = build.GetComponent<Health>();
                        if (hp != null && hp.IsAlive) hp.Heal(1f);
                    };
                    break;

                case PassiveKind.SharperTeeth:
                    stats.AddDamageMultiplier(passive.tags, 1.10f);
                    break;

                case PassiveKind.Swift:
                    stats.moveSpeed *= 1.05f;
                    break;

                case PassiveKind.QuickBiter:
                    stats.passiveBiteIntervalSeconds *= 0.9f;
                    break;

                case PassiveKind.PulseOverlap:
                    foreach (var inst in build.Combat.Instances)
                    {
                        if (inst is PulseInstance pi) pi.Mutations.RadiusMultiplier *= 1.25f;
                    }
                    break;

                case PassiveKind.FieldLingering:
                    foreach (var inst in build.Combat.Instances)
                    {
                        if (inst is FieldInstance fi) fi.Mutations.DurationMultiplier *= 1.30f;
                    }
                    break;

                case PassiveKind.Endurance:
                {
                    float factor = 1f + Mathf.Max(0f, passive.magnitude);
                    stats.maxHp *= factor;
                    var hp = build.GetComponent<Health>();
                    if (hp != null) hp.SetMaxHp(stats.maxHp, applyDelta: true);
                    break;
                }

                case PassiveKind.Ferocity:
                {
                    float factor = 1f + Mathf.Max(0f, passive.magnitude);
                    stats.globalDamageMultiplier *= factor;
                    break;
                }

                case PassiveKind.ApexHunger:
                    build.XP.AddXPMultiplier(1.20f);
                    break;

                case PassiveKind.Voidburst:
                    EventBus.OnLevelUp += _ =>
                    {
                        if (build == null) return;
                        var origin = (Vector2)build.transform.position;
                        var hits = Physics2D.OverlapCircleAll(origin, 12f);
                        foreach (var col in hits)
                        {
                            if (col == null) continue;
                            if (!col.TryGetComponent(out EnemyController enemy)) continue;
                            var hp = enemy.GetComponent<Health>();
                            if (hp == null || !hp.IsAlive) continue;
                            build.Combat.DealDamage(hp, 50f, passive.tags, passive);
                        }
                    };
                    break;

                // Stubs — tag already tracked via passive definition; effect deferred.
                case PassiveKind.AcidSpray:
                case PassiveKind.Corrosion:
                case PassiveKind.SwarmRend:
                case PassiveKind.ConsumingWave:
                    Debug.Log($"[Passive] {passive.displayName} picked (effect stub — tags recorded).");
                    break;
            }
        }
    }
}
