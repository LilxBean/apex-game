using System.Collections.Generic;
using UnityEngine;

namespace APEX.Enemies.Data
{
    /// <summary>
    /// Defines an era's spawn table and spawn rate curve.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Era_New", menuName = "APEX/Eras/Era Definition")]
    public class EraDefinition : ScriptableObject
    {
        public string id;
        public string displayName;

        [Tooltip("0-8, mapping to the nine eras of existence.")]
        public int index;

        public List<SpawnEntry> spawnTable;

        [Tooltip("X = seconds into run, Y = spawns per second.")]
        public AnimationCurve spawnRateOverTime = AnimationCurve.Linear(0f, 0.5f, 180f, 4f);
    }
}
