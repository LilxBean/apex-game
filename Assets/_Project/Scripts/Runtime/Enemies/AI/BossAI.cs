using UnityEngine;

namespace APEX.Enemies.AI
{
    /// <summary>
    /// Chases the player with massive HP.
    /// TODO: Boss loop should be authored per-era with phase transitions, special attacks, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_AI_Boss", menuName = "APEX/Enemies/AI/Boss")]
    public class BossAI : EnemyAIBehaviour
    {
        public override void Tick(EnemyController self, float dt)
        {
            if (self.Target == null) return;

            Vector2 toPlayer = self.Target.Position - (Vector2)self.transform.position;
            self.Move(toPlayer);
        }
    }
}
