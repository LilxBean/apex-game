using APEX.Tags;

namespace APEX.Combat.Attacks
{
    /// <summary>
    /// Per-player runtime for a single attack slot. Ticks cooldown, fires on demand.
    /// </summary>
    public interface IAttackInstance
    {
        TagSet Tags { get; }
        AttackDefinition Definition { get; }
        float Cooldown { get; }
        bool AutoFire { get; set; }

        void Tick(float dt);
        void Fire(bool manual = false);
        void Cleanup();
    }
}
