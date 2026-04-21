using System;

namespace APEX.Core
{
    /// <summary>
    /// Snapshot of run results emitted on EventBus.OnRunEnded. Populated by RunManager
    /// as the run progresses; consumers read this instead of reaching back into RunManager.
    /// </summary>
    [Serializable]
    public struct RunStats
    {
        public float RunDurationSeconds;
        public int LevelReached;
        public int Kills;
        public float DamageDealt;
        public float PeakDPS;
        public int XPTotal;
        public int PicksTaken;
        public int PassivesTaken;
        public int HammersTaken;

        public float AverageDPS => RunDurationSeconds > 0.01f ? DamageDealt / RunDurationSeconds : 0f;
    }
}
