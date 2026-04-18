using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Spore / Acid Spit — pooled projectile, homes on nearest enemy.
    /// Tags: [Projectile][Acid]. Manual fire uses mouse; auto fire never reads cursor.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Attack_Spit", menuName = "APEX/Attacks/Spit")]
    public class SpitAttack : AttackDefinition
    {
        public GameObject projectilePrefab;
        public float spitProjectileSpeed = 12f;
        public float spitProjectileLifetime = 2.5f;
        public float spitAcquireRadius = 12f;

        public override IAttackInstance CreateInstance(PlayerCombat combat) => new SpitInstance(this, combat);
    }
}
