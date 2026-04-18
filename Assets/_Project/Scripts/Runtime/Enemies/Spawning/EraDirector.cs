using APEX.Combat;
using APEX.Enemies.Data;
using UnityEngine;

namespace APEX.Enemies.Spawning
{
    /// <summary>
    /// Owns the current era, drives spawn timing via the era's spawn curve.
    /// </summary>
    public class EraDirector : MonoBehaviour
    {
        [SerializeField] private EraDefinition _currentEra;
        [SerializeField] private EnemySpawner _spawner;

        private float _runTime;
        private float _spawnAccumulator;
        private IPlayerTarget _playerTarget;

        public float RunTime => _runTime;
        public EraDefinition CurrentEra => _currentEra;

        public float CurrentSpawnRate
        {
            get
            {
                if (_currentEra == null || _currentEra.spawnRateOverTime == null) return 0f;
                return _currentEra.spawnRateOverTime.Evaluate(_runTime);
            }
        }

        private void Start()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb is IPlayerTarget target)
                {
                    _playerTarget = target;
                    break;
                }
            }

            if (_playerTarget == null)
            {
                Debug.LogError("[EraDirector] No IPlayerTarget found in scene.");
                enabled = false;
                return;
            }

            _spawner.Initialize(_playerTarget);
            SetEra(_currentEra);
        }

        public void SetEra(EraDefinition era)
        {
            _currentEra = era;
            _spawner.SetEra(era);
        }

        private void Update()
        {
            if (_currentEra == null) return;

            _runTime += Time.deltaTime;

            float rate = _currentEra.spawnRateOverTime.Evaluate(_runTime);
            _spawnAccumulator += rate * Time.deltaTime;

            while (_spawnAccumulator >= 1f)
            {
                _spawnAccumulator -= 1f;
                _spawner.SpawnEnemy();
            }
        }
    }
}
