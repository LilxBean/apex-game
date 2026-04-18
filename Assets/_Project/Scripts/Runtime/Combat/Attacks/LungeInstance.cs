using System.Collections;
using System.Collections.Generic;
using APEX.Enemies;
using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    public class LungeInstance : AttackInstanceBase
    {
        private static readonly Collider2D[] _overlapBuffer = new Collider2D[32];
        private readonly LungeAttack _def;

        public LungeInstance(LungeAttack def, PlayerCombat combat) : base(def, combat)
        {
            _def = def;
        }

        protected override void DoFire(bool manual)
        {
            Vector2 origin = Combat.Controller.Position;
            var nearest = Combat.FindNearestEnemy(origin, _def.lungeAcquireRadius);

            Vector2 dir;
            if (nearest != null)
            {
                dir = ((Vector2)nearest.transform.position - origin).normalized;
            }
            else
            {
                dir = Combat.Controller.FacingDirection;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
            }

            Combat.Controller.SetMovementLockout(_def.lungeDurationSeconds);
            Combat.RunCoroutine(Dash(dir));
        }

        private IEnumerator Dash(Vector2 dir)
        {
            var rb = Combat.Controller.Rigidbody;
            var sprite = Combat.Controller.SpriteRenderer;
            float speed = _def.lungeDistance / Mathf.Max(_def.lungeDurationSeconds, 0.001f);

            rb.linearVelocity = dir * speed;

            Vector3 originalScale = sprite.transform.localScale;
            Vector3 stretched = new Vector3(
                Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ? originalScale.x * 1.5f : originalScale.x * 0.85f,
                Mathf.Abs(dir.y) > Mathf.Abs(dir.x) ? originalScale.y * 1.5f : originalScale.y * 0.85f,
                originalScale.z);
            sprite.transform.localScale = stretched;

            var alreadyHit = new HashSet<EnemyController>();
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float elapsed = 0f;

            while (elapsed < _def.lungeDurationSeconds)
            {
                Vector2 playerPos = Combat.Controller.Position;
                Vector2 boxCenter = playerPos + dir * (_def.lungeHitboxSize.x * 0.5f);
                int count = Physics2D.OverlapBoxNonAlloc(boxCenter, _def.lungeHitboxSize, angle, _overlapBuffer);

                for (int i = 0; i < count; i++)
                {
                    var col = _overlapBuffer[i];
                    if (col == null) continue;
                    if (!col.TryGetComponent(out EnemyController enemy)) continue;
                    if (alreadyHit.Contains(enemy)) continue;
                    var hp = enemy.GetComponent<Health>();
                    if (hp == null || !hp.IsAlive) continue;

                    alreadyHit.Add(enemy);
                    Combat.DealDamage(hp, Def.baseDamage, Def.tags, this);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            sprite.transform.localScale = originalScale;
        }
    }
}
