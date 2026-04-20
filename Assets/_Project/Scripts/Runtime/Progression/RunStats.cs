using System;

namespace APEX.Progression
{
    /// <summary>
    /// Snapshot of run results emitted on EventBus.OnRunEnded. Populated by RunManager
    /// at run-end. Consumers read this instead of reaching back into RunManager.
    /// </summary>
    [Serializable]
    public struct RunStats
    {
        public float RunDurationSeconds;
        public int LevelReached;

        // TODO session 3: kills, DPS peak, XP total, picks taken, hammers taken
    }
}
