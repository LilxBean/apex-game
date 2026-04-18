using System;
using APEX.Combat;
using APEX.Enemies;

namespace APEX.Core.Events
{
    /// <summary>
    /// Static event bus for cross-system hooks. Subscribers must unsubscribe in OnDisable.
    /// </summary>
    public static class EventBus
    {
        public static event Action<IDamageable, Damage> OnDamaged;
        public static event Action<EnemyController, Damage> OnEnemyKilled;

        public static void RaiseDamaged(IDamageable target, Damage damage)
        {
            OnDamaged?.Invoke(target, damage);
        }

        public static void RaiseEnemyKilled(EnemyController enemy, Damage damage)
        {
            OnEnemyKilled?.Invoke(enemy, damage);
        }
    }
}
