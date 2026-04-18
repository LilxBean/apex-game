using APEX.Core.Pooling;
using APEX.Player;
using APEX.Tags;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Pooled projectile. Despawns on first damageable hit (not the player) or after lifetime.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class SporeProjectile : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private PlayerCombat _combat;
        private PrefabPool _pool;
        private float _damage;
        private TagSet _tags;
        private float _lifetimeRemaining;
        private bool _live;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        public void Init(Vector2 dir, float speed, float lifetime, float damage, TagSet tags, PlayerCombat combat)
        {
            _combat = combat;
            _pool = combat.Pool;
            _damage = damage;
            _tags = tags;
            _lifetimeRemaining = lifetime;
            _rb.linearVelocity = dir * speed;
            _live = true;
        }

        private void Update()
        {
            if (!_live) return;
            _lifetimeRemaining -= Time.deltaTime;
            if (_lifetimeRemaining <= 0f)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_live) return;
            if (other.TryGetComponent(out IPlayerTarget _)) return; // don't damage the player
            if (!other.TryGetComponent(out IDamageable target)) return;

            _combat.DealDamage(target, _damage, _tags, this);
            Despawn();
        }

        private void Despawn()
        {
            _live = false;
            _rb.linearVelocity = Vector2.zero;
            if (_pool != null)
            {
                _pool.Release(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
