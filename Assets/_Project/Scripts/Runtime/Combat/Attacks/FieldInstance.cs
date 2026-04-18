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
            field.Init(_def.fieldRadius, _def.fieldDurationSeconds, _def.fieldTickIntervalSeconds,
                Def.baseDamage, Def.tags, Combat, this);
        }
    }
}
