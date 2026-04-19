using System;
using APEX.Core.Events;
using APEX.Tags;
using UnityEngine;

namespace APEX.Combat
{
    /// <summary>
    /// Shared health component used by enemies (and eventually the player).
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHp = 100f;
        [SerializeField] private TagSet _resistances;
        [SerializeField] private TagSet _weaknesses;

        private float _currentHp;

        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        public bool IsAlive => _currentHp > 0f;

        public event Action<Damage> Died;

        private void OnEnable()
        {
            _currentHp = _maxHp;
        }

        public void Initialize(float maxHp, TagSet resistances, TagSet weaknesses)
        {
            _maxHp = maxHp;
            _resistances = resistances;
            _weaknesses = weaknesses;
            _currentHp = _maxHp;
        }

        public void ResetHealth()
        {
            _currentHp = _maxHp;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || !IsAlive) return;
            _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
        }

        public void TakeDamage(in Damage damage)
        {
            if (!IsAlive) return;

            float multiplier = ComputeMultiplier(damage.Tags);
            float finalDamage = damage.Amount * multiplier;

            _currentHp = Mathf.Max(0f, _currentHp - finalDamage);

            EventBus.RaiseDamaged(this, damage);

            if (_currentHp <= 0f)
            {
                Died?.Invoke(damage);
            }
        }

        private float ComputeMultiplier(TagSet incomingTags)
        {
            int resistCount = _resistances.IntersectionCount(incomingTags);
            int weakCount = _weaknesses.IntersectionCount(incomingTags);

            if (resistCount > 0 && weakCount == 0) return 0.5f;
            if (weakCount > 0 && resistCount == 0) return 1.5f;
            return 1f; // both present or neither → neutral
        }
    }
}
