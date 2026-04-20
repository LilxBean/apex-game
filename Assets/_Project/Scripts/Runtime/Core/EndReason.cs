namespace APEX.Core
{
    /// <summary>
    /// Why a run stopped. Populated by RunManager and carried on EventBus.OnRunEnded so UI
    /// can branch on the cause (defeat vs. victory vs. player-initiated quit).
    /// </summary>
    public enum EndReason
    {
        Victory,
        Defeat,
        Quit
    }
}
