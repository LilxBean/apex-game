using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    public class SpitInstance : AttackInstanceBase
    {
        private readonly SpitAttack _def;

        public SpitInstance(SpitAttack def, PlayerCombat combat) : base(def, combat)
        {
            _def = def;
        }

        protected override void DoFire(bool manual)
        {
            if (_def.projectilePrefab == null)
            {
                Debug.LogWarning("[SpitInstance] Missing projectilePrefab on SpitAttack SO.");
                return;
            }

            Vector2 origin = Combat.Controller.Position;
            Vector2 dir = ResolveAim(origin, manual);
            if (dir.sqrMagnitude < 0.0001f) return;
            dir.Normalize();

            var go = Combat.Pool.Get(_def.projectilePrefab);
            go.transform.position = origin;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);

            if (!go.TryGetComponent(out SporeProjectile proj))
            {
                proj = go.AddComponent<SporeProjectile>();
            }
            proj.Init(dir, _def.spitProjectileSpeed, _def.spitProjectileLifetime,
                EffectiveDamage, Tags, Combat);
        }

        private Vector2 ResolveAim(Vector2 origin, bool manual)
        {
            var nearest = Combat.FindNearestEnemy(origin, _def.spitAcquireRadius);
            if (nearest != null)
            {
                return ((Vector2)nearest.transform.position - origin);
            }

            if (manual)
            {
                var mouseWorld = Combat.TryGetMouseWorldPosition();
                if (mouseWorld.HasValue)
                {
                    Vector2 toMouse = mouseWorld.Value - origin;
                    if (toMouse.sqrMagnitude > 0.0001f) return toMouse;
                }
            }

            Vector2 facing = Combat.Controller.FacingDirection;
            return facing.sqrMagnitude > 0.0001f ? facing : Vector2.right;
        }
    }
}
