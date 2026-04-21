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

        // Progression: amount gained, xp into current level, xp required for current level, fraction 0..1.
        public static event Action<int, int, int, float> OnXPGained;
        public static event Action<int> OnLevelUp;
        public static event Action<PickKind> OnPickTaken;
        public static event Action<RunStats, EndReason> OnRunEnded;
        public static event Action OnPaused;
        public static event Action OnResumed;

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

        public static void RaiseXPGained(int amount, int intoLevel, int requiredForLevel, float fraction)
        {
            OnXPGained?.Invoke(amount, intoLevel, requiredForLevel, fraction);
        }

        public static void RaiseLevelUp(int newLevel)
        {
            OnLevelUp?.Invoke(newLevel);
        }

        public static void RaisePickTaken(PickKind kind)
        {
            OnPickTaken?.Invoke(kind);
        }

        public static void RaiseRunEnded(RunStats stats, EndReason reason)
        {
            OnRunEnded?.Invoke(stats, reason);
        }

        public static void RaisePaused()
        {
            OnPaused?.Invoke();
        }

        public static void RaiseResumed()
        {
            OnResumed?.Invoke();
        }
    }
}
