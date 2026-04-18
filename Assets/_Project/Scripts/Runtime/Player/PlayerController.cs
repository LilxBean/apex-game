using APEX.Combat;
using APEX.Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace APEX.Player
{
    /// <summary>
    /// The real player: movement, health, death. Implements IPlayerTarget so enemy AI
    /// locates this object, and IDamageable so contact damage routes through Health.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class PlayerController : MonoBehaviour, IPlayerTarget, IDamageable
    {
        [SerializeField] private PlayerStats _stats;

        private Rigidbody2D _rb;
        private Health _health;
        private SpriteRenderer _spriteRenderer;

        private APEXControls _controls;
        private InputAction _moveAction;

        private Vector2 _facingDirection = Vector2.right;
        private bool _simFrozen;
        private float _movementLockout;

        public Vector2 Position => (Vector2)transform.position;
        public PlayerStats Stats => _stats;
        public Vector2 FacingDirection => _facingDirection;
        public Health Health => _health;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public Rigidbody2D Rigidbody => _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;

            _controls = new APEXControls();
            _moveAction = _controls.Gameplay.Move;

            if (_stats != null)
            {
                _health.Initialize(_stats.maxHp, default, default);
            }
        }

        private void OnEnable()
        {
            _controls.Gameplay.Enable();
            _controls.Combat.Enable();
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _controls.Gameplay.Disable();
            _controls.Combat.Disable();
            _health.Died -= OnDied;
        }

        private void OnDestroy()
        {
            _controls?.Dispose();
        }

        private void FixedUpdate()
        {
            if (_simFrozen) return;

            if (_movementLockout > 0f)
            {
                _movementLockout -= Time.fixedDeltaTime;
                // Still update facing from input so attack aim stays responsive.
                Vector2 raw = _moveAction.ReadValue<Vector2>();
                if (raw.sqrMagnitude > 0.0001f) _facingDirection = raw.normalized;
                return;
            }

            Vector2 input = _moveAction.ReadValue<Vector2>();
            if (input.sqrMagnitude > 1f) input.Normalize();

            if (input.sqrMagnitude > 0.0001f)
            {
                _facingDirection = input.normalized;
            }

            float speed = _stats != null ? _stats.moveSpeed : 6f;
            _rb.linearVelocity = input * speed;
        }

        /// <summary>
        /// Suspend movement input for a short window (used by Lunge so the dash velocity
        /// isn't overwritten by FixedUpdate).
        /// </summary>
        public void SetMovementLockout(float seconds)
        {
            if (seconds > _movementLockout) _movementLockout = seconds;
        }

        public void TakeDamage(in Damage damage)
        {
            _health.TakeDamage(damage);
        }

        private void OnDied(Damage killingBlow)
        {
            if (_simFrozen) return;
            _simFrozen = true;
            _rb.linearVelocity = Vector2.zero;
            Time.timeScale = 0f;
            Debug.Log("[Player] Run ended.");
            EventBus.RaisePlayerDied(this, killingBlow);
        }
    }
}
