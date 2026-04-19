using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// XP-per-level curve, expressed as a geometric growth from a base. Level 1 is the
    /// starting level; <see cref="GetXPForLevel"/>(1) returns the XP needed to reach level 2.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_XPCurve", menuName = "APEX/Progression/XP Curve")]
    public class XPCurve : ScriptableObject
    {
        [Tooltip("XP required to go from level 1 to level 2.")]
        public float xpBase = 10f;

        [Tooltip("Each level requires this * previous level's XP. 1.15 = +15% per level.")]
        public float growth = 1.15f;

        [Tooltip("Highest level the curve is defined for.")]
        public int maxLevel = 40;

        /// <summary>XP needed to advance from <paramref name="currentLevel"/> to the next.</summary>
        public int GetXPForLevel(int currentLevel)
        {
            if (currentLevel < 1) currentLevel = 1;
            if (currentLevel >= maxLevel) return int.MaxValue;
            float v = xpBase * Mathf.Pow(growth, currentLevel - 1);
            return Mathf.Max(1, Mathf.RoundToInt(v));
        }
    }
}
