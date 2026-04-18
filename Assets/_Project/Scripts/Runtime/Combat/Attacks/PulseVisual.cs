using APEX.Core.Pooling;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Purely visual. Scales 0 → target radius over duration, then returns to pool.
    /// </summary>
    public class PulseVisual : MonoBehaviour
    {
        private float _targetRadius;
        private float _duration;
        private float _elapsed;
        private PrefabPool _pool;
        private bool _playing;

        public void Play(float radius, float duration, PrefabPool pool)
        {
            _targetRadius = radius;
            _duration = Mathf.Max(duration, 0.01f);
            _elapsed = 0f;
            _pool = pool;
            _playing = true;
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (!_playing) return;
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            // visual diameter = 2x radius; prefab sprite is assumed 1-unit
            float scale = 2f * _targetRadius * t;
            transform.localScale = new Vector3(scale, scale, 1f);

            if (t >= 1f)
            {
                _playing = false;
                if (_pool != null) _pool.Release(gameObject);
                else gameObject.SetActive(false);
            }
        }
    }
}
