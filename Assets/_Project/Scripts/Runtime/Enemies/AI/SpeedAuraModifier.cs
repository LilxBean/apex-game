namespace APEX.Enemies.AI
{
    /// <summary>
    /// +25% moveSpeed while aura is active.
    /// </summary>
    public class SpeedAuraModifier : ISwarmModifier
    {
        private readonly float _multiplier;

        public SpeedAuraModifier(float multiplier = 1.25f)
        {
            _multiplier = multiplier;
        }

        public void Apply(EnemyController target)
        {
            target.ApplySpeedModifier(_multiplier);
        }

        public void Remove(EnemyController target)
        {
            target.RemoveSpeedModifier(_multiplier);
        }
    }
}
