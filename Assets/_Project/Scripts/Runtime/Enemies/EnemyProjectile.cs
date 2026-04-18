using APEX.Combat;
using APEX.Tags;
using UnityEngine;

namespace APEX.Enemies
{
    /// <summary>
    /// Simple straight-line projectile fired by ranged enemies.
    /// TODO: Pool these instead of Instantiate/Destroy.
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _lifetime;
        private float _elapsed;

        public void Initialize(Vector2 direction, float speed, float damage, float lifetime)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _lifetime = lifetime;
            _elapsed = 0f;
        }

        private void Update()
        {
            transform.Translate(_direction * (_speed * Time.deltaTime), Space.World);

            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
            {
                Destroy(gameObject); // TODO: return to pool
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IPlayerTarget _) &&
                other.TryGetComponent(out IDamageable damageable))
            {
                var damage = new Damage(_damage, new TagSet(null), this);
                damageable.TakeDamage(damage);
                Destroy(gameObject); // TODO: return to pool
            }
        }
    }
}
