using System;
using APEX.Combat;
using APEX.Core.Events;
using APEX.Enemies.AI;
using APEX.Enemies.Data;
using APEX.Tags;
using UnityEngine;

namespace APEX.Enemies
{
    /// <summary>
    /// Runtime controller on every enemy prefab. Reads EnemyDefinition, drives AI, handles death.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemyController : MonoBehaviour
    {
        private EnemyDefinition _definition;
        private Health _health;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private EnemyAIBehaviour _ai;

        private float _baseMoveSpeed;
        private float _moveSpeedModifier = 1f;

        // Per-instance AI state (shared SO can't hold this)
        [NonSerialized] public float AITimer;

        public IPlayerTarget Target { get; private set; }
        public EnemyDefinition Definition => _definition;
        public float EffectiveMoveSpeed => _baseMoveSpeed * _moveSpeedModifier;
        public float ContactDamage => _definition != null ? _definition.contactDamage : 0f;

        /// <summary>Spawner subscribes to know when to release back to pool.</summary>
        public event Action<EnemyController> ReturnToPool;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(EnemyDefinition definition, IPlayerTarget target)
        {
            _definition = definition;
            Target = target;
            _baseMoveSpeed = definition.moveSpeed;
            _moveSpeedModifier = 1f;
            AITimer = 0f;

            _health.Initialize(definition.maxHp, definition.resistances, definition.weaknesses);
            _spriteRenderer.color = definition.debugColor;
            _ai = definition.aiBehaviour;

            _health.Died += OnDied;
        }

        private void Update()
        {
            if (_ai != null && _health.IsAlive)
            {
                _ai.Tick(this, Time.deltaTime);
            }
        }

        public void Move(Vector2 direction, float speedScale = 1f)
        {
            _rb.linearVelocity = direction.normalized * (EffectiveMoveSpeed * speedScale);
        }

        public void ApplySpeedModifier(float multiplier)
        {
            _moveSpeedModifier *= multiplier;
        }

        public void RemoveSpeedModifier(float multiplier)
        {
            if (multiplier != 0f)
                _moveSpeedModifier /= multiplier;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_health.IsAlive) return;
            if (collision.gameObject.TryGetComponent(out IDamageable damageable) &&
                collision.gameObject.TryGetComponent(out IPlayerTarget _))
            {
                var damage = new Damage(ContactDamage, new TagSet(null), this);
                damageable.TakeDamage(damage);
            }
        }

        private void OnDied(Damage killingBlow)
        {
            _health.Died -= OnDied;
            _rb.linearVelocity = Vector2.zero;
            _ai?.OnReset(this);

            EventBus.RaiseEnemyKilled(this, killingBlow);
            ReturnToPool?.Invoke(this);
        }

        public void ResetForPool()
        {
            _rb.linearVelocity = Vector2.zero;
            _moveSpeedModifier = 1f;
            AITimer = 0f;
            _ai?.OnReset(this);
            _ai = null;
            _definition = null;
            Target = null;
            ReturnToPool = null;
        }
    }
}
