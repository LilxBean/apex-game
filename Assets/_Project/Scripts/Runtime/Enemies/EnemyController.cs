using System;
using System.Collections;
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
        private Collider2D _collider;
        private EnemyAIBehaviour _ai;

        private float _baseMoveSpeed;
        private float _moveSpeedModifier = 1f;
        private float _knockbackLockout;

        private Color _baseColor = Color.white;
        private Coroutine _flashCo;
        private Coroutine _popCo;

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
            _collider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            EventBus.OnDamaged += OnAnyDamaged;
        }

        private void OnDisable()
        {
            EventBus.OnDamaged -= OnAnyDamaged;
        }

        public void Initialize(EnemyDefinition definition, IPlayerTarget target)
        {
            _definition = definition;
            Target = target;
            _baseMoveSpeed = definition.moveSpeed;
            _moveSpeedModifier = 1f;
            AITimer = 0f;

            _health.Initialize(definition.maxHp, definition.resistances, definition.weaknesses);
            _baseColor = definition.debugColor;
            _spriteRenderer.color = _baseColor;
            _ai = definition.aiBehaviour;

            _health.Died += OnDied;
        }

        private void Update()
        {
            if (_knockbackLockout > 0f)
            {
                _knockbackLockout -= Time.deltaTime;
                return;
            }

            if (_ai != null && _health.IsAlive)
            {
                _ai.Tick(this, Time.deltaTime);
            }
        }

        public void Move(Vector2 direction, float speedScale = 1f)
        {
            _rb.linearVelocity = direction.normalized * (EffectiveMoveSpeed * speedScale);
        }

        /// <summary>
        /// Apply an instantaneous velocity and suspend AI movement for a short lockout
        /// so the push isn't overwritten next frame.
        /// </summary>
        public void ApplyKnockback(Vector2 impulse, float lockoutSeconds)
        {
            _rb.linearVelocity = impulse;
            _knockbackLockout = Mathf.Max(_knockbackLockout, lockoutSeconds);
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
            if (_collider != null) _collider.enabled = false;

            EventBus.RaiseEnemyKilled(this, killingBlow);

            if (_flashCo != null) { StopCoroutine(_flashCo); _flashCo = null; }
            if (_popCo != null) StopCoroutine(_popCo);
            _popCo = StartCoroutine(DeathPopRoutine());
        }

        private void OnAnyDamaged(IDamageable target, Damage damage)
        {
            if ((object)target != _health) return;
            if (!_health.IsAlive) return;
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            const float holdSeconds = 0.05f;
            const float fadeSeconds = 0.08f;
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(holdSeconds);

            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeSeconds);
                _spriteRenderer.color = Color.Lerp(Color.white, _baseColor, k);
                yield return null;
            }
            _spriteRenderer.color = _baseColor;
            _flashCo = null;
        }

        private IEnumerator DeathPopRoutine()
        {
            const float scaleSeconds = 0.06f;
            const float fadeSeconds = 0.12f;
            const float peakScale = 1.35f;

            Vector3 baseScale = Vector3.one;
            Color startColor = _spriteRenderer.color;

            float t = 0f;
            while (t < scaleSeconds)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / scaleSeconds);
                transform.localScale = Vector3.Lerp(baseScale, baseScale * peakScale, k);
                yield return null;
            }

            t = 0f;
            Vector3 fromScale = transform.localScale;
            while (t < fadeSeconds)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeSeconds);
                transform.localScale = Vector3.Lerp(fromScale, baseScale * (peakScale * 0.9f), k);
                var c = startColor; c.a = 1f - k;
                _spriteRenderer.color = c;
                yield return null;
            }

            _popCo = null;
            ReturnToPool?.Invoke(this);
        }

        public void ResetForPool()
        {
            if (_flashCo != null) { StopCoroutine(_flashCo); _flashCo = null; }
            if (_popCo != null) { StopCoroutine(_popCo); _popCo = null; }
            _rb.linearVelocity = Vector2.zero;
            _moveSpeedModifier = 1f;
            _knockbackLockout = 0f;
            AITimer = 0f;
            _ai?.OnReset(this);
            _ai = null;
            _definition = null;
            Target = null;
            transform.localScale = Vector3.one;
            if (_collider != null) _collider.enabled = true;
            var c = _spriteRenderer.color; c.a = 1f; _spriteRenderer.color = c;
            ReturnToPool = null;
        }
    }
}
