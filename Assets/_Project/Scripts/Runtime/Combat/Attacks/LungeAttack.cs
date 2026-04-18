using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Lunge / Consume — physics-driven dash with a forward-facing box hitbox.
    /// Tags: [Melee][Physical].
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Attack_Lunge", menuName = "APEX/Attacks/Lunge")]
    public class LungeAttack : AttackDefinition
    {
        public float lungeDistance = 4f;
        public float lungeDurationSeconds = 0.18f;
        public Vector2 lungeHitboxSize = new(2f, 1.2f);
        public float lungeAcquireRadius = 6f;

        public override IAttackInstance CreateInstance(PlayerCombat combat) => new LungeInstance(this, combat);
    }
}
