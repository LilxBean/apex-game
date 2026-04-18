using System.Collections.Generic;
using APEX.Combat;
using APEX.Core.Events;
using APEX.Enemies;
using APEX.Enemies.Spawning;
using TMPro;
using UnityEngine;

namespace APEX.Testing
{
    /// <summary>
    /// On-screen debug text: enemies alive, elapsed time, spawn rate, kills this run,
    /// and DPS averaged over the last 5 seconds of player-sourced hits.
    /// </summary>
    public class SandboxHUD : MonoBehaviour
    {
        private const float DpsWindowSeconds = 5f;

        [SerializeField] private TextMeshProUGUI _debugText;
        [SerializeField] private EraDirector _director;

        private int _enemiesAlive;
        private int _killCount;

        private readonly Queue<HitSample> _recentHits = new();
        private float _rollingDamage;

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += HandleEnemyKilled;
            EventBus.OnPlayerHitEnemy += HandlePlayerHit;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= HandleEnemyKilled;
            EventBus.OnPlayerHitEnemy -= HandlePlayerHit;
        }

        private void Update()
        {
            _enemiesAlive = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
            PruneOldHits();

            if (_debugText == null || _director == null) return;

            float dps = _rollingDamage / DpsWindowSeconds;
            _debugText.text =
                $"Enemies: {_enemiesAlive}\n" +
                $"Time: {_director.RunTime:F1}s\n" +
                $"Spawn Rate: {_director.CurrentSpawnRate:F2}/s\n" +
                $"Kills: {_killCount}\n" +
                $"DPS (5s): {dps:F1}";
        }

        private void HandleEnemyKilled(EnemyController enemy, Damage damage)
        {
            _killCount++;
        }

        private void HandlePlayerHit(IDamageable target, Damage damage)
        {
            _recentHits.Enqueue(new HitSample { Time = Time.time, Amount = damage.Amount });
            _rollingDamage += damage.Amount;
        }

        private void PruneOldHits()
        {
            float cutoff = Time.time - DpsWindowSeconds;
            while (_recentHits.Count > 0 && _recentHits.Peek().Time < cutoff)
            {
                var old = _recentHits.Dequeue();
                _rollingDamage -= old.Amount;
            }
            if (_rollingDamage < 0f) _rollingDamage = 0f;
        }

        private struct HitSample
        {
            public float Time;
            public float Amount;
        }
    }
}
