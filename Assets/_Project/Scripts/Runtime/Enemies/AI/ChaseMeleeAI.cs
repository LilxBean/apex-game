using UnityEngine;

namespace APEX.Enemies.AI
{
    /// <summary>
    /// Steers directly at the player. Contact damage is handled by EnemyController collision.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_AI_ChaseMelee", menuName = "APEX/Enemies/AI/Chase Melee")]
    public class ChaseMeleeAI : EnemyAIBehaviour
    {
        public override void Tick(EnemyController self, float dt)
        {
            if (self.Target == null) return;

            Vector2 direction = self.Target.Position - (Vector2)self.transform.position;
            self.Move(direction);
        }
    }
}
