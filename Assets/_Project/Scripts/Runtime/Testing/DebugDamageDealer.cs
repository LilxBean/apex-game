using APEX.Combat;
using APEX.Enemies;
using APEX.Tags;
using UnityEngine;
using UnityEngine.InputSystem;

namespace APEX.Testing
{
    /// <summary>
    /// Press F to deal 50 damage to the nearest enemy. Debug tool only.
    /// </summary>
    public class DebugDamageDealer : MonoBehaviour
    {
        [SerializeField] private float _damage = 50f;
        [SerializeField] private float _range = 50f;

        private APEXControls _controls;
        private InputAction _debugAttackAction;

        private static readonly Collider2D[] _overlapBuffer = new Collider2D[64];

        private void Awake()
        {
            _controls = new APEXControls();
            _debugAttackAction = _controls.Gameplay.DebugAttack;
        }

        private void OnEnable()
        {
            _controls.Gameplay.Enable();
            _debugAttackAction.performed += OnDebugAttack;
        }

        private void OnDisable()
        {
            _debugAttackAction.performed -= OnDebugAttack;
            _controls.Gameplay.Disable();
        }

        private void OnDestroy() => _controls.Dispose();

        private void OnDebugAttack(InputAction.CallbackContext ctx)
        {
            int count = Physics2D.OverlapCircleNonAlloc(
                transform.position, _range, _overlapBuffer);

            EnemyController nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (_overlapBuffer[i].TryGetComponent(out EnemyController enemy) &&
                    enemy.GetComponent<Health>().IsAlive)
                {
                    float dist = Vector2.Distance(transform.position, enemy.transform.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = enemy;
                    }
                }
            }

            if (nearest != null)
            {
                var damage = new Damage(_damage, new TagSet(null), this);
                nearest.GetComponent<Health>().TakeDamage(damage);
            }
        }
    }
}
