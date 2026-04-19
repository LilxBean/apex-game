using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Pickup that magnets toward the player when inside pickup radius, grants XP on contact.
    /// Pooled via PrefabPool; returned through XPOrbSpawner.Release.
    /// </summary>
    public class XPOrb : MonoBehaviour
    {
        [SerializeField] private float _pickupRadius = 0.6f;
        [SerializeField] private float _magnetRadius = 3.5f;
        [SerializeField] private float _magnetSpeed = 10f;

        private int _value;
        private Transform _playerTransform;
        private PlayerXP _playerXP;
        private XPOrbSpawner _owner;
        private bool _alive;

        public void Initialize(int value, Transform player, PlayerXP playerXP, XPOrbSpawner owner)
        {
            _value = Mathf.Max(1, value);
            _playerTransform = player;
            _playerXP = playerXP;
            _owner = owner;
            _alive = true;
        }

        private void Update()
        {
            if (!_alive || _playerTransform == null) return;

            Vector2 orbPos = transform.position;
            Vector2 playerPos = _playerTransform.position;
            Vector2 toPlayer = playerPos - orbPos;
            float sqr = toPlayer.sqrMagnitude;

            if (sqr <= _pickupRadius * _pickupRadius)
            {
                _alive = false;
                _playerXP?.GainXP(_value);
                _owner?.Release(gameObject);
                return;
            }

            if (sqr <= _magnetRadius * _magnetRadius)
            {
                Vector2 dir = toPlayer.normalized;
                transform.position = orbPos + dir * (_magnetSpeed * Time.deltaTime);
            }
        }
    }
}
