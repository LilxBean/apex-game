using System;
using System.Collections.Generic;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Per-level-band composition of the level-up pick screen. Reads the player's current
    /// level and returns the rarity and hammer probabilities to roll with.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_PickTable", menuName = "APEX/Progression/Pick Table")]
    public class PickTable : ScriptableObject
    {
        [SerializeField] private List<LevelBand> _bands = new();

        public LevelBand GetBandFor(int level)
        {
            LevelBand chosen = _bands.Count > 0 ? _bands[0] : default;
            for (int i = 0; i < _bands.Count; i++)
            {
                if (level >= _bands[i].minLevel) chosen = _bands[i];
            }
            return chosen;
        }

        public void SetBands(List<LevelBand> bands)
        {
            _bands = bands;
        }

        [Serializable]
        public struct LevelBand
        {
            public string name;
            public int minLevel;
            [Range(0f, 1f)] public float commonPct;
            [Range(0f, 1f)] public float rarePct;
            [Range(0f, 1f)] public float epicPct;
            [Range(0f, 1f)] public float hammerPct;
        }
    }
}
