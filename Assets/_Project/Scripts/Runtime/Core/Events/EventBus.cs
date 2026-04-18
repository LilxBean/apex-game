using System;
using APEX.Combat;
using APEX.Enemies;
using APEX.Player;

namespace APEX.Core.Events
{
    /// <summary>
    /// Static event bus for cross-system hooks. Subscribers must unsubscribe in OnDisable.
    /// </summary>
    public static class EventBus
    {
        public static event Action<IDamageable, Damage> OnDamaged;
        public static event Action<EnemyController, Damage> OnEnemyKilled;
        public static event Action<PlayerController, Damage> OnPlayerDied;
        public static event Action<IDamageable, Damage> OnPlayerHitEnemy;

        public static void RaiseDamaged(IDamageable target, Damage damage)
        {
            OnDamaged?.Invoke(target, damage);
        }

        public static void RaiseEnemyKilled(EnemyController enemy, Damage damage)
        {
            OnEnemyKilled?.Invoke(enemy, damage);
        }

        public static void RaisePlayerDied(PlayerController player, Damage damage)
        {
            OnPlayerDied?.Invoke(player, damage);
        }

        public static void RaisePlayerHitEnemy(IDamageable target, Damage damage)
        {
            OnPlayerHitEnemy?.Invoke(target, damage);
        }
    }
}
