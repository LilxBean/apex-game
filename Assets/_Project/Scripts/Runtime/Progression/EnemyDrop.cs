using APEX.Combat;
using APEX.Enemies;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Sits on the enemy prefab. On death, asks XPOrbSpawner to drop this enemy's XP
    /// as orbs at the death position. Reads xpDrop + orbCount from EnemyDefinition.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(Health))]
    public class EnemyDrop : MonoBehaviour
    {
        private EnemyController _enemy;
        private Health _health;

        private void Awake()
        {
            _enemy = GetComponent<EnemyController>();
            _health = GetComponent<Health>();
        }

        private void OnEnable() { _health.Died += OnDied; }
        private void OnDisable() { _health.Died -= OnDied; }

        private void OnDied(Damage _)
        {
            var def = _enemy.Definition;
            if (def == null) return;
            if (def.xpDrop <= 0) return;
            var spawner = XPOrbSpawner.Instance;
            if (spawner == null) return;
            spawner.Spawn(transform.position, def.xpDrop, Mathf.Max(1, def.orbCount));
        }
    }
}
