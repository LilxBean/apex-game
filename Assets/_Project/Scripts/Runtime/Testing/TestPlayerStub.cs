// TEMP — replaced by APEX.Player.PlayerController in the player session.
using APEX.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace APEX.Testing
{
    /// <summary>
    /// Minimal player stand-in for testing enemies. WASD movement + IPlayerTarget for AI.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TestPlayerStub : MonoBehaviour, IPlayerTarget, IDamageable
    {
        [SerializeField] private float _moveSpeed = 6f;

        private APEXControls _controls;
        private InputAction _moveAction;
        private Rigidbody2D _rb;

        public Vector2 Position => (Vector2)transform.position;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _controls = new APEXControls();
            _moveAction = _controls.Gameplay.Move;
        }

        private void OnEnable() => _controls.Gameplay.Enable();
        private void OnDisable() => _controls.Gameplay.Disable();
        private void OnDestroy() => _controls.Dispose();

        private void FixedUpdate()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            if (input.sqrMagnitude > 1f)
                input.Normalize();

            _rb.linearVelocity = input * _moveSpeed;
        }

        public void TakeDamage(in Damage damage)
        {
            Debug.Log($"[PlayerStub] Took {damage.Amount} damage from {damage.Source}");
        }
    }
}
