using APEX.Player;
using APEX.Tags;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Default cooldown + fire-guard implementation. Subclasses override DoFire.
    /// </summary>
    public abstract class AttackInstanceBase : IAttackInstance
    {
        protected readonly PlayerCombat Combat;
        protected readonly AttackDefinition Def;
        protected float CooldownRemaining;

        public readonly AttackMutations Mutations = new();
        public readonly System.Collections.Generic.HashSet<string> AppliedHammerIds = new();

        protected AttackInstanceBase(AttackDefinition def, PlayerCombat combat)
        {
            Def = def;
            Combat = combat;
            AutoFire = def.autoFireByDefault;
        }

        public TagSet Tags => Mutations.BuildTags(Def.tags);
        public AttackDefinition Definition => Def;
        public float Cooldown => CooldownRemaining;
        public bool AutoFire { get; set; }

        protected float EffectiveDamage => Def.baseDamage * Mutations.DamageMultiplier;

        public virtual void Tick(float dt)
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining -= dt;
            }

            if (AutoFire && CooldownRemaining <= 0f)
            {
                Fire(false);
            }
        }

        public void Fire(bool manual = false)
        {
            if (CooldownRemaining > 0f) return;
            CooldownRemaining = Def.baseCooldownSeconds * Mutations.CooldownMultiplier;
            DoFire(manual);
        }

        protected abstract void DoFire(bool manual);

        public virtual void Cleanup() { }
    }
}
