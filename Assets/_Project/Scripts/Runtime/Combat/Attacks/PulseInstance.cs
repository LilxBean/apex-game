using System.Collections;
using APEX.Enemies;
using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    public class PulseInstance : AttackInstanceBase
    {
        private static readonly Collider2D[] _overlapBuffer = new Collider2D[64];
        private readonly PulseAttack _def;

        public PulseInstance(PulseAttack def, PlayerCombat combat) : base(def, combat)
        {
            _def = def;
        }

        protected override void DoFire(bool manual)
        {
            Vector2 origin = Combat.Controller.Position;

            float radius = _def.pulseRadius * Mutations.RadiusMultiplier;

            if (Def.vfxPrefab != null)
            {
                var vfx = Combat.Pool.Get(Def.vfxPrefab);
                vfx.transform.position = origin;
                if (!vfx.TryGetComponent(out PulseVisual visual))
                {
                    visual = vfx.AddComponent<PulseVisual>();
                }
                visual.Play(radius, _def.visualDurationSeconds, Combat.Pool);
            }

            Combat.RunCoroutine(ResolveAfterWindup(origin, radius));
        }

        private IEnumerator ResolveAfterWindup(Vector2 origin, float radius)
        {
            if (_def.windupSeconds > 0f)
            {
                yield return new WaitForSeconds(_def.windupSeconds);
            }

            int count = Physics2D.OverlapCircleNonAlloc(origin, radius, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent(out EnemyController enemy)) continue;
                var hp = enemy.GetComponent<Health>();
                if (hp == null || !hp.IsAlive) continue;

                Vector2 away = ((Vector2)enemy.transform.position - origin);
                if (away.sqrMagnitude < 0.0001f) away = Vector2.right;
                away.Normalize();

                Combat.DealDamage(hp, EffectiveDamage, Tags, this);
                enemy.ApplyKnockback(away * _def.knockbackImpulse, 0.15f);
            }
        }
    }
}
