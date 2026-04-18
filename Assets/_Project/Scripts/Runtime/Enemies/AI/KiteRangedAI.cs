using UnityEngine;

namespace APEX.Enemies.AI
{
    /// <summary>
    /// Maintains preferred distance from player, fires placeholder projectiles on cooldown.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_AI_KiteRanged", menuName = "APEX/Enemies/AI/Kite Ranged")]
    public class KiteRangedAI : EnemyAIBehaviour
    {
        [Header("Kiting")]
        public float preferredDistance = 6f;

        [Header("Firing")]
        public float fireCooldown = 2f;
        public float projectileSpeed = 8f;
        public float projectileDamage = 5f;
        public float projectileLifetime = 4f;
        public GameObject projectilePrefab;

        public override void Tick(EnemyController self, float dt)
        {
            if (self.Target == null) return;

            Vector2 toPlayer = self.Target.Position - (Vector2)self.transform.position;
            float distance = toPlayer.magnitude;

            // Kite movement
            if (distance < preferredDistance)
            {
                self.Move(-toPlayer); // move away
            }
            else if (distance > preferredDistance * 1.2f)
            {
                self.Move(toPlayer); // close in
            }
            else
            {
                self.Move(Vector2.zero); // hold position
            }

            // Fire on cooldown
            self.AITimer += dt;
            if (self.AITimer >= fireCooldown)
            {
                self.AITimer = 0f;
                Fire(self, toPlayer.normalized);
            }
        }

        private void Fire(EnemyController self, Vector2 direction)
        {
            if (projectilePrefab == null) return;

            // TODO: pool projectiles instead of Instantiate
            var go = Object.Instantiate(projectilePrefab, self.transform.position, Quaternion.identity);
            var proj = go.GetComponent<EnemyProjectile>();
            if (proj != null)
            {
                proj.Initialize(direction, projectileSpeed, projectileDamage, projectileLifetime);
            }
        }

        public override void OnReset(EnemyController self)
        {
            self.AITimer = 0f;
        }
    }
}
