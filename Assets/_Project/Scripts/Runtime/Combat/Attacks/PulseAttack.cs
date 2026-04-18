using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Radial Pulse — brief windup, then damage + knockback on everything in radius.
    /// Tags: [Nova][Physical].
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Attack_Pulse", menuName = "APEX/Attacks/Pulse")]
    public class PulseAttack : AttackDefinition
    {
        public float windupSeconds = 0.15f;
        public float pulseRadius = 4.5f;
        public float knockbackImpulse = 8f;
        public float visualDurationSeconds = 0.3f;

        public override IAttackInstance CreateInstance(PlayerCombat combat) => new PulseInstance(this, combat);
    }
}
