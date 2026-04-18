using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace APEX.Core.Pooling
{
    /// <summary>
    /// Manages ObjectPools keyed by prefab reference. No raw Instantiate/Destroy in hot paths.
    /// </summary>
    public class PrefabPool
    {
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();

        private const int DefaultCapacity = 20;
        private const int MaxSize = 200;

        public GameObject Get(GameObject prefab)
        {
            return GetOrCreatePool(prefab).Get();
        }

        public void Release(GameObject instance)
        {
            if (instance.TryGetComponent(out PoolTag tag))
            {
                GetOrCreatePool(tag.Prefab).Release(instance);
            }
            else
            {
                Debug.LogWarning($"[PrefabPool] Tried to release {instance.name} but it has no PoolTag.");
                Object.Destroy(instance);
            }
        }

        private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                var captured = prefab;
                pool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        var obj = Object.Instantiate(captured);
                        var tag = obj.AddComponent<PoolTag>();
                        tag.Prefab = captured;
                        obj.SetActive(false);
                        return obj;
                    },
                    actionOnGet: obj => obj.SetActive(true),
                    actionOnRelease: obj => obj.SetActive(false),
                    actionOnDestroy: Object.Destroy,
                    defaultCapacity: DefaultCapacity,
                    maxSize: MaxSize
                );
                _pools[prefab] = pool;
            }
            return pool;
        }
    }

    /// <summary>
    /// Tracks which prefab an instance was spawned from, so Release can find the right pool.
    /// </summary>
    [AddComponentMenu("")]
    public class PoolTag : MonoBehaviour
    {
        [HideInInspector] public GameObject Prefab;
    }
}
