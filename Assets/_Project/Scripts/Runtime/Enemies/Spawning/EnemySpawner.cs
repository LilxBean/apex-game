using APEX.Combat;
using APEX.Core.Pooling;
using APEX.Enemies.Data;
using UnityEngine;

namespace APEX.Enemies.Spawning
{
    /// <summary>
    /// Spawns enemies from the current era's spawn table using weighted random selection.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float _spawnRadius = 15f;

        private EraDefinition _currentEra;
        private IPlayerTarget _playerTarget;
        private PrefabPool _pool;

        public void Initialize(IPlayerTarget playerTarget)
        {
            _playerTarget = playerTarget;
            _pool = new PrefabPool();
        }

        public void SetEra(EraDefinition era)
        {
            _currentEra = era;
        }

        public void SpawnEnemy()
        {
            if (_currentEra == null || _currentEra.spawnTable == null || _currentEra.spawnTable.Count == 0)
                return;

            var entry = PickWeightedRandom();
            if (entry.definition == null || entry.definition.prefab == null) return;

            Vector2 spawnPos = GetSpawnPosition();

            var go = _pool.Get(entry.definition.prefab);
            go.transform.position = spawnPos;

            var controller = go.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(entry.definition, _playerTarget);
                controller.ReturnToPool += OnEnemyReturnToPool;
            }
        }

        private SpawnEntry PickWeightedRandom()
        {
            float totalWeight = 0f;
            var table = _currentEra.spawnTable;

            for (int i = 0; i < table.Count; i++)
                totalWeight += table[i].weight;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < table.Count; i++)
            {
                cumulative += table[i].weight;
                if (roll <= cumulative)
                    return table[i];
            }

            return table[table.Count - 1];
        }

        private Vector2 GetSpawnPosition()
        {
            Vector2 center = _playerTarget?.Position ?? Vector2.zero;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _spawnRadius;
        }

        private void OnEnemyReturnToPool(EnemyController controller)
        {
            controller.ReturnToPool -= OnEnemyReturnToPool;
            controller.ResetForPool();
            _pool.Release(controller.gameObject);
        }
    }
}
