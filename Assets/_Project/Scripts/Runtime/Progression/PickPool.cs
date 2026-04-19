using System.Collections.Generic;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Catalog of all passives and hammers available this run. Referenced by the pick roller.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_PickPool", menuName = "APEX/Progression/Pick Pool")]
    public class PickPool : ScriptableObject
    {
        public List<PassiveDefinition> passives = new();
        public List<HammerDefinition> hammers = new();
    }
}
