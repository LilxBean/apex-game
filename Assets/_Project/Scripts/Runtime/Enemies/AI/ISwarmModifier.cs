namespace APEX.Enemies.AI
{
    /// <summary>
    /// A modifier that an Elite can apply to nearby swarm enemies.
    /// </summary>
    public interface ISwarmModifier
    {
        void Apply(EnemyController target);
        void Remove(EnemyController target);
    }
}
