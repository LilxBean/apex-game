using APEX.Player;
using UnityEngine;

namespace APEX.Combat.Attacks
{
    public class FieldInstance : AttackInstanceBase
    {
        private readonly FieldAttack _def;

        public FieldInstance(FieldAttack def, PlayerCombat combat) : base(def, combat)
        {
            _def = def;
        }

        protected override void DoFire(bool manual)
        {
            if (_def.fieldPrefab == null)
            {
                Debug.LogWarning("[FieldInstance] Missing fieldPrefab on FieldAttack SO.");
                return;
            }

            Vector2 origin = Combat.Controller.Position;
            var go = Combat.Pool.Get(_def.fieldPrefab);
            go.transform.position = origin;

            if (!go.TryGetComponent(out CorrosiveField field))
            {
                field = go.AddComponent<CorrosiveField>();
            }
            float radius = _def.fieldRadius * Mutations.RadiusMultiplier;
            float duration = _def.fieldDurationSeconds * Mutations.DurationMultiplier;
            field.Init(radius, duration, _def.fieldTickIntervalSeconds,
                EffectiveDamage, Tags, Combat, this);
        }
    }
}
