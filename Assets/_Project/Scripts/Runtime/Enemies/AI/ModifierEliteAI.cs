using System.Collections.Generic;
using UnityEngine;

namespace APEX.Enemies.AI
{
    /// <summary>
    /// Chases the player like melee. Periodically applies SpeedAuraModifier to nearby allies.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_AI_ModifierElite", menuName = "APEX/Enemies/AI/Modifier Elite")]
    public class ModifierEliteAI : EnemyAIBehaviour
    {
        public float auraRadius = 5f;
        public int maxAffected = 5;
        public float retargetInterval = 1f;
        public float speedMultiplier = 1.25f;

        // Per-instance tracking via static dictionary keyed by instance ID.
        // Cleaned up in OnReset.
        private static readonly Dictionary<int, HashSet<EnemyController>> _auraTargets = new();
        private static readonly Dictionary<int, SpeedAuraModifier> _modifiers = new();

        private static readonly Collider2D[] _overlapBuffer = new Collider2D[32];

        public override void Tick(EnemyController self, float dt)
        {
            if (self.Target == null) return;

            // Chase like melee
            Vector2 toPlayer = self.Target.Position - (Vector2)self.transform.position;
            self.Move(toPlayer);

            // Retarget aura on interval
            self.AITimer += dt;
            if (self.AITimer >= retargetInterval)
            {
                self.AITimer = 0f;
                UpdateAura(self);
            }
        }

        private void UpdateAura(EnemyController self)
        {
            int id = self.GetInstanceID();

            if (!_auraTargets.TryGetValue(id, out var currentTargets))
            {
                currentTargets = new HashSet<EnemyController>();
                _auraTargets[id] = currentTargets;
            }

            if (!_modifiers.TryGetValue(id, out var modifier))
            {
                modifier = new SpeedAuraModifier(speedMultiplier);
                _modifiers[id] = modifier;
            }

            // Find nearby enemies
            int count = Physics2D.OverlapCircleNonAlloc(
                self.transform.position, auraRadius, _overlapBuffer);

            var newTargets = new HashSet<EnemyController>();
            int affected = 0;

            for (int i = 0; i < count && affected < maxAffected; i++)
            {
                if (_overlapBuffer[i].TryGetComponent(out EnemyController other) &&
                    other != self &&
                    other.gameObject.activeInHierarchy)
                {
                    newTargets.Add(other);
                    affected++;

                    if (!currentTargets.Contains(other))
                    {
                        modifier.Apply(other);
                    }
                }
            }

            // Remove aura from enemies that left range
            foreach (var prev in currentTargets)
            {
                if (prev != null && !newTargets.Contains(prev))
                {
                    modifier.Remove(prev);
                }
            }

            _auraTargets[id] = newTargets;
        }

        public override void OnReset(EnemyController self)
        {
            int id = self.GetInstanceID();

            if (_auraTargets.TryGetValue(id, out var targets))
            {
                if (_modifiers.TryGetValue(id, out var modifier))
                {
                    foreach (var target in targets)
                    {
                        if (target != null)
                            modifier.Remove(target);
                    }
                }
                _auraTargets.Remove(id);
            }
            _modifiers.Remove(id);
            self.AITimer = 0f;
        }
    }
}
