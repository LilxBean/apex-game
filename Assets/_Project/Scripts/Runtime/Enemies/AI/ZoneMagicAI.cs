using UnityEngine;

namespace APEX.Enemies.AI
{
    /// <summary>
    /// Slow drift toward player. TODO: periodically drops a persistent area effect at own feet.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_AI_ZoneMagic", menuName = "APEX/Enemies/AI/Zone Magic")]
    public class ZoneMagicAI : EnemyAIBehaviour
    {
        [Tooltip("Movement speed multiplier relative to definition moveSpeed.")]
        public float driftSpeedScale = 0.5f;

        public override void Tick(EnemyController self, float dt)
        {
            if (self.Target == null) return;

            Vector2 toPlayer = self.Target.Position - (Vector2)self.transform.position;
            self.Move(toPlayer, driftSpeedScale);
        }
    }
}
