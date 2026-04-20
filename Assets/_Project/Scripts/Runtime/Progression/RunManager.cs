using APEX.Combat;
using APEX.Core;
using APEX.Core.Events;
using APEX.Enemies;
using APEX.Player;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Tracks run time, kills, and level reached. Ends the run on player death or when the
    /// run timer hits runLengthSeconds. Raises EventBus.OnRunEnded so UI can show the end screen.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private float _runLengthSeconds = 15f * 60f;
        [SerializeField] private PlayerXP _playerXP;

        private float _runTime;
        private int _kills;
        private bool _ended;

        public float RunTime => _runTime;
        public int Kills => _kills;
        public int LevelReached => _playerXP != null ? _playerXP.Level : 1;
        public bool Ended => _ended;

        public void Bind(PlayerXP xp)
        {
            _playerXP = xp;
        }

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += OnEnemyKilled;
            EventBus.OnPlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= OnEnemyKilled;
            EventBus.OnPlayerDied -= OnPlayerDied;
        }

        private void Update()
        {
            if (_ended) return;
            _runTime += Time.deltaTime;
            // TODO session 3: timer expiry should emit EndReason.Victory once win detection exists.
            if (_runTime >= _runLengthSeconds) EndRun(EndReason.Defeat);
        }

        private void OnEnemyKilled(EnemyController _, Damage __) => _kills++;

        private void OnPlayerDied(PlayerController _, Damage __) => EndRun(EndReason.Defeat);

        private void EndRun(EndReason reason)
        {
            if (_ended) return;
            _ended = true;
            Time.timeScale = 0f;

            var stats = new RunStats
            {
                RunDurationSeconds = _runTime,
                LevelReached = _playerXP != null ? _playerXP.Level : 1
            };
            EventBus.RaiseRunEnded(stats, reason);
        }
    }
}
