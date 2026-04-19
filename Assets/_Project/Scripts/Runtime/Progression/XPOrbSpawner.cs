using APEX.Core.Pooling;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Scene-level service that owns the orb prefab, the pool, and the player references.
    /// EnemyDrop calls Spawn(); XPOrb calls Release(). Assigned in the Run scene.
    /// </summary>
    public class XPOrbSpawner : MonoBehaviour
    {
        public static XPOrbSpawner Instance { get; private set; }

        [SerializeField] private GameObject _orbPrefab;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private PlayerXP _playerXP;

        private PrefabPool _pool;

        private void Awake()
        {
            Instance = this;
            _pool = new PrefabPool();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Spawn <paramref name="orbCount"/> orbs whose XP values sum to <paramref name="totalXP"/>.</summary>
        public void Spawn(Vector3 origin, int totalXP, int orbCount)
        {
            if (_orbPrefab == null || _playerTransform == null || _playerXP == null) return;
            if (totalXP <= 0) return;
            if (orbCount <= 0) orbCount = 1;

            int remaining = totalXP;
            for (int i = 0; i < orbCount; i++)
            {
                int orbValue = (i == orbCount - 1) ? remaining : Mathf.Max(1, totalXP / orbCount);
                remaining -= orbValue;
                if (orbValue <= 0) break;

                var go = _pool.Get(_orbPrefab);
                Vector2 jitter = Random.insideUnitCircle * 0.35f;
                go.transform.position = origin + new Vector3(jitter.x, jitter.y, 0f);
                go.transform.rotation = Quaternion.identity;

                if (!go.TryGetComponent(out XPOrb orb)) orb = go.AddComponent<XPOrb>();
                orb.Initialize(orbValue, _playerTransform, _playerXP, this);
            }
        }

        public void Release(GameObject instance)
        {
            _pool.Release(instance);
        }
    }
}
