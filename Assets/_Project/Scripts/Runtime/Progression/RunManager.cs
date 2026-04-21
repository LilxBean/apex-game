using APEX.Combat;
using APEX.Core;
using APEX.Core.Events;
using APEX.Enemies;
using APEX.Player;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Owns the run lifecycle and aggregates RunStats via EventBus subscriptions. Ends the run on
    /// player death (Defeat) or timer expiry (Victory, if the player is still alive). Raises
    /// EventBus.OnRunEnded so UI / persistence can react.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private float _runLengthSeconds = 15f * 60f;
        [SerializeField] private PlayerXP _playerXP;

        private Health _playerHealth;
        private float _runTime;
        private bool _ended;

        private RunStats _stats;
        private readonly DamageWindow _dpsWindow = new();
        private float _frameDamage;

        public float RunTime => _runTime;
        public int LevelReached => _playerXP != null ? _playerXP.Level : 1;
        public bool Ended => _ended;

        public void Bind(PlayerXP xp, Health playerHealth = null)
        {
            _playerXP = xp;
            _playerHealth = playerHealth;
        }

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += OnEnemyKilled;
            EventBus.OnPlayerDied += OnPlayerDied;
            EventBus.OnPlayerHitEnemy += OnPlayerHitEnemy;
            EventBus.OnXPGained += OnXPGained;
            EventBus.OnPickTaken += OnPickTaken;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= OnEnemyKilled;
            EventBus.OnPlayerDied -= OnPlayerDied;
            EventBus.OnPlayerHitEnemy -= OnPlayerHitEnemy;
            EventBus.OnXPGained -= OnXPGained;
            EventBus.OnPickTaken -= OnPickTaken;
        }

        private void Update()
        {
            if (_ended) return;
            _runTime += Time.deltaTime;

            _dpsWindow.Tick(_runTime, _frameDamage);
            _frameDamage = 0f;

            if (_runTime >= _runLengthSeconds)
            {
                bool playerAlive = _playerHealth == null || _playerHealth.IsAlive;
                EndRun(playerAlive ? EndReason.Victory : EndReason.Defeat);
            }
        }

        private void OnEnemyKilled(EnemyController _, Damage __) => _stats.Kills++;

        private void OnPlayerDied(PlayerController _, Damage __) => EndRun(EndReason.Defeat);

        private void OnPlayerHitEnemy(IDamageable _, Damage damage)
        {
            _stats.DamageDealt += damage.Amount;
            _frameDamage += damage.Amount;
        }

        private void OnXPGained(int amount, int _, int __, float ___) => _stats.XPTotal += amount;

        private void OnPickTaken(PickKind kind)
        {
            _stats.PicksTaken++;
            if (kind == PickKind.Hammer) _stats.HammersTaken++;
            else _stats.PassivesTaken++;
        }

        /// <summary>
        /// Builds the RunStats snapshot from the current aggregated state. Used by the event-driven
        /// run-end path and by callers that want to peek at stats without ending the run.
        /// </summary>
        public RunStats BuildStats()
        {
            _stats.RunDurationSeconds = _runTime;
            _stats.LevelReached = _playerXP != null ? _playerXP.Level : 1;
            _stats.PeakDPS = _dpsWindow.Peak;
            return _stats;
        }

        /// <summary>
        /// Public run-end entry for external callers (pause overlay's End Run / Quit).
        /// Idempotent — additional calls after the first are no-ops.
        /// </summary>
        public void EndRun(EndReason reason)
        {
            if (_ended) return;
            _ended = true;
            Time.timeScale = 0f;

            var stats = BuildStats();
            EventBus.RaiseRunEnded(stats, reason);
        }

        // Rolling-window peak DPS tracker. Five one-second buckets; peak = max over the run of
        // (sum / window size). Kept private — consumers read stats.PeakDPS, not this helper.
        private sealed class DamageWindow
        {
            // Tunable: DPS peak window size
            private const int WindowSize = 5;

            private readonly float[] _buckets = new float[WindowSize];
            private int _currentSecond = -1;
            private float _peak;

            public float Peak => _peak;

            public void Tick(float elapsed, float frameDamage)
            {
                int second = Mathf.FloorToInt(elapsed);
                if (_currentSecond < 0)
                {
                    _currentSecond = second;
                }
                else if (second != _currentSecond)
                {
                    int advances = Mathf.Min(second - _currentSecond, WindowSize);
                    for (int i = 0; i < advances; i++)
                    {
                        for (int j = WindowSize - 1; j > 0; j--) _buckets[j] = _buckets[j - 1];
                        _buckets[0] = 0f;
                    }
                    _currentSecond = second;
                }

                _buckets[0] += frameDamage;

                float sum = 0f;
                for (int i = 0; i < WindowSize; i++) sum += _buckets[i];
                float dps = sum / WindowSize;
                if (dps > _peak) _peak = dps;
            }
        }
    }
}
