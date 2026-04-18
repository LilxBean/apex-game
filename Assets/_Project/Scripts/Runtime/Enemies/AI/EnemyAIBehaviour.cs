using UnityEngine;

namespace APEX.Enemies.AI
{
    /// <summary>
    /// Abstract base for polymorphic AI behaviours authored as ScriptableObjects.
    /// Each EnemyDefinition references one of these.
    /// </summary>
    public abstract class EnemyAIBehaviour : ScriptableObject
    {
        public abstract void Tick(EnemyController self, float dt);

        /// <summary>Called when the enemy is returned to pool. Clean up per-instance state.</summary>
        public virtual void OnReset(EnemyController self) { }
    }
}
