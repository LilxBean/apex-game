using APEX.Combat;
using APEX.Core.Events;
using UnityEngine;

namespace APEX.CameraRig
{
    /// <summary>
    /// Smooth follow camera for the 2D orthographic main camera. Tracks a target
    /// with SmoothDamp and adds a small velocity-driven look-ahead so bursts of
    /// motion (e.g. Lunge) stay visible without whipping the frame. Optional
    /// rectangular clamp keeps the viewport inside an arena. Adds trauma-based
    /// screen shake on hits to a configured IDamageable (typically the player).
    ///
    /// Namespace is APEX.CameraRig (not APEX.Camera) so importers can still
    /// reference the Unity type `Camera` without a naming collision.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Rigidbody2D _targetRb;

        [Header("Smoothing")]
        [SerializeField] private float _smoothTime = 0.18f;
        [SerializeField] private float _lookAheadScale = 0.12f;
        [SerializeField] private float _maxLookAhead = 2f;

        [Header("Clamp (optional)")]
        [SerializeField] private bool _useClamp;
        [SerializeField] private Rect _clampBounds;

        [Header("Shake")]
        [SerializeField] private float _maxShakeOffset = 0.9f;
        [SerializeField] private float _traumaDecayPerSecond = 2.2f;
        [SerializeField] private float _shakeFrequency = 28f;

        private Camera _camera;
        private Vector3 _velocityCache;
        private Vector3 _smoothedPos;
        private float _trauma;
        private float _shakeSeed;

        private IDamageable _shakeTarget;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _smoothedPos = transform.position;
            _shakeSeed = Random.value * 100f;
        }

        private void OnEnable()
        {
            EventBus.OnDamaged += OnAnyDamaged;
        }

        private void OnDisable()
        {
            EventBus.OnDamaged -= OnAnyDamaged;
        }

        public void Configure(Transform target, Rigidbody2D targetRb, float smoothTime)
        {
            _target = target;
            _targetRb = targetRb;
            _smoothTime = smoothTime;
        }

        public void SetClamp(Rect worldRect)
        {
            _clampBounds = worldRect;
            _useClamp = true;
        }

        public void ClearClamp()
        {
            _useClamp = false;
        }

        /// <summary>
        /// Subscribe to damage events on a specific IDamageable (the player's Health)
        /// and add trauma when that target is hit.
        /// </summary>
        public void ConfigureShakeTarget(IDamageable shakeTarget)
        {
            _shakeTarget = shakeTarget;
        }

        /// <summary>Manual trauma injection (0..1). Use for non-damage events like era advance.</summary>
        public void AddTrauma(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        public void SnapToTarget()
        {
            if (_target == null) return;
            Vector3 desired = ComputeDesired();
            _smoothedPos = new Vector3(desired.x, desired.y, 0f);
            transform.position = new Vector3(desired.x, desired.y, transform.position.z);
            _velocityCache = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = ComputeDesired();
            _smoothedPos = Vector3.SmoothDamp(
                _smoothedPos,
                new Vector3(desired.x, desired.y, 0f),
                ref _velocityCache,
                Mathf.Max(_smoothTime, 0.0001f));

            Vector2 shake = Vector2.zero;
            if (_trauma > 0f)
            {
                _trauma = Mathf.Max(0f, _trauma - _traumaDecayPerSecond * Time.unscaledDeltaTime);
                float amp = _maxShakeOffset * _trauma * _trauma;
                float time = Time.unscaledTime * _shakeFrequency;
                shake = new Vector2(
                    (Mathf.PerlinNoise(_shakeSeed + time, 0f) - 0.5f) * 2f,
                    (Mathf.PerlinNoise(0f, _shakeSeed + time) - 0.5f) * 2f) * amp;
            }

            transform.position = new Vector3(_smoothedPos.x + shake.x, _smoothedPos.y + shake.y, transform.position.z);
        }

        private void OnAnyDamaged(IDamageable target, Damage damage)
        {
            if (_shakeTarget == null || (object)target != (object)_shakeTarget) return;
            float traumaAmount = Mathf.Clamp(damage.Amount * 0.04f, 0.25f, 0.9f);
            AddTrauma(traumaAmount);
        }

        private Vector3 ComputeDesired()
        {
            Vector2 pos = _target.position;

            if (_targetRb != null)
            {
                Vector2 look = _targetRb.linearVelocity * _lookAheadScale;
                if (look.sqrMagnitude > _maxLookAhead * _maxLookAhead)
                {
                    look = look.normalized * _maxLookAhead;
                }
                pos += look;
            }

            if (_useClamp && _camera != null)
            {
                float halfH = _camera.orthographicSize;
                float halfW = halfH * _camera.aspect;

                float minX = _clampBounds.xMin + halfW;
                float maxX = _clampBounds.xMax - halfW;
                float minY = _clampBounds.yMin + halfH;
                float maxY = _clampBounds.yMax - halfH;

                pos.x = minX > maxX ? _clampBounds.center.x : Mathf.Clamp(pos.x, minX, maxX);
                pos.y = minY > maxY ? _clampBounds.center.y : Mathf.Clamp(pos.y, minY, maxY);
            }

            return new Vector3(pos.x, pos.y, 0f);
        }
    }
}
