using APEX.Combat;
using APEX.Core.Events;
using APEX.Enemies;
using APEX.Enemies.Spawning;
using TMPro;
using UnityEngine;

namespace APEX.Testing
{
    /// <summary>
    /// On-screen debug text: enemies alive, elapsed time, spawn rate.
    /// </summary>
    public class SandboxHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _debugText;
        [SerializeField] private EraDirector _director;

        private int _enemiesAlive;

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += HandleEnemyKilled;
            EventBus.OnDamaged += HandleDamaged;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= HandleEnemyKilled;
            EventBus.OnDamaged -= HandleDamaged;
        }

        private void Update()
        {
            // Count active enemies each frame for accuracy (pool reuse can drift a counter)
            _enemiesAlive = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;

            if (_debugText == null || _director == null) return;

            _debugText.text =
                $"Enemies: {_enemiesAlive}\n" +
                $"Time: {_director.RunTime:F1}s\n" +
                $"Spawn Rate: {_director.CurrentSpawnRate:F2}/s";
        }

        private void HandleEnemyKilled(EnemyController enemy, Damage damage) { }
        private void HandleDamaged(IDamageable target, Damage damage) { }
    }
}
