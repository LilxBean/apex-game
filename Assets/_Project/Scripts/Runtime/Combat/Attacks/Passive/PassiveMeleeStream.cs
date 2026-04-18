using APEX.Enemies;
using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks.Passive
{
    /// <summary>
    /// Always-on melee damage around the player. Not cooldown-gated in the attack-slot sense:
    /// ticks on a fixed interval from PlayerStats, outside the four-attack framework.
    /// </summary>
    public class PassiveMeleeStream : MonoBehaviour
    {
        private static readonly Collider2D[] _overlapBuffer = new Collider2D[32];

        private PlayerController _controller;
        private PlayerCombat _combat;
        private float _timer;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _combat = GetComponent<PlayerCombat>();
        }

        private void Update()
        {
            var stats = _controller != null ? _controller.Stats : null;
            if (stats == null || !stats.passiveStreamEnabled) return;

            _timer += Time.deltaTime;
            if (_timer < stats.passiveBiteIntervalSeconds) return;
            _timer -= stats.passiveBiteIntervalSeconds;

            Vector2 origin = _controller.Position;
            int count = Physics2D.OverlapCircleNonAlloc(origin, stats.passiveBiteRadius, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent(out EnemyController enemy)) continue;
                var hp = enemy.GetComponent<Health>();
                if (hp == null || !hp.IsAlive) continue;

                _combat.DealDamage(hp, stats.passiveBiteDamage, stats.passiveBiteTags, this);
            }
        }
    }
}
