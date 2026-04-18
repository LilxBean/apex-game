using System.Collections.Generic;
using APEX.Core.Pooling;
using APEX.Player;
using APEX.Tags;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Persistent AoE. Tracks damageables inside its trigger and ticks them on interval.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class CorrosiveField : MonoBehaviour
    {
        private const float FadeSeconds = 0.25f;

        private CircleCollider2D _collider;
        private SpriteRenderer _sprite;
        private PrefabPool _pool;
        private PlayerCombat _combat;
        private object _source;

        private readonly HashSet<IDamageable> _occupants = new();
        private float _radius;
        private float _duration;
        private float _tickInterval;
        private float _dps;
        private TagSet _tags;

        private float _elapsed;
        private float _tickTimer;
        private float _fadeElapsed;
        private bool _live;
        private Color _baseColor;

        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;
            _sprite = GetComponent<SpriteRenderer>();
        }

        public void Init(float radius, float duration, float tickInterval, float dps,
            TagSet tags, PlayerCombat combat, object source)
        {
            _radius = radius;
            _duration = duration;
            _tickInterval = Mathf.Max(0.05f, tickInterval);
            _dps = dps;
            _tags = tags;
            _combat = combat;
            _pool = combat.Pool;
            _source = source;

            // Assume the prefab sprite is 1 unit; scale to 2*radius so it matches the trigger.
            _collider.radius = 1f;
            transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

            _baseColor = _sprite.color;
            var c = _baseColor; c.a = _baseColor.a;
            _sprite.color = c;

            _occupants.Clear();
            _elapsed = 0f;
            _tickTimer = 0f;
            _fadeElapsed = 0f;
            _live = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_live) return;
            if (other.TryGetComponent(out IPlayerTarget _)) return;
            if (other.TryGetComponent(out IDamageable dmg))
            {
                _occupants.Add(dmg);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out IDamageable dmg))
            {
                _occupants.Remove(dmg);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_live)
            {
                _elapsed += dt;
                _tickTimer += dt;

                if (_tickTimer >= _tickInterval)
                {
                    _tickTimer -= _tickInterval;
                    Tick();
                }

                if (_elapsed >= _duration)
                {
                    _live = false;
                    _fadeElapsed = 0f;
                }
            }
            else
            {
                _fadeElapsed += dt;
                float t = Mathf.Clamp01(_fadeElapsed / FadeSeconds);
                var c = _baseColor;
                c.a = _baseColor.a * (1f - t);
                _sprite.color = c;

                if (t >= 1f)
                {
                    Despawn();
                }
            }
        }

        private void Tick()
        {
            if (_occupants.Count == 0) return;

            float perTick = _dps * _tickInterval;
            // Copy to array so expired enemies can't mutate the set mid-iteration.
            var snapshot = new IDamageable[_occupants.Count];
            _occupants.CopyTo(snapshot);

            for (int i = 0; i < snapshot.Length; i++)
            {
                var target = snapshot[i];
                if (target == null)
                {
                    _occupants.Remove(target);
                    continue;
                }
                _combat.DealDamage(target, perTick, _tags, _source);
            }
        }

        private void Despawn()
        {
            _occupants.Clear();
            _sprite.color = _baseColor;
            if (_pool != null) _pool.Release(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
