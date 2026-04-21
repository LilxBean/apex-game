namespace APEX.Core.Events
{
    /// <summary>
    /// Category of a level-up pick. Carried on EventBus.OnPickTaken so stat aggregators
    /// can distinguish passives from hammers without knowing the full pick payload.
    /// </summary>
    public enum PickKind
    {
        Passive,
        Hammer
    }
}
