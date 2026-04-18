using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Corrosive Field — persistent AoE that ticks damage on a timer.
    /// baseDamage is damage-per-second; per-tick damage = baseDamage * fieldTickIntervalSeconds.
    /// Tags: [Area][Persistent][Acid].
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Attack_Field", menuName = "APEX/Attacks/Field")]
    public class FieldAttack : AttackDefinition
    {
        public GameObject fieldPrefab;
        public float fieldRadius = 2.5f;
        public float fieldDurationSeconds = 4f;
        public float fieldTickIntervalSeconds = 0.5f;

        public override IAttackInstance CreateInstance(PlayerCombat combat) => new FieldInstance(this, combat);
    }
}
