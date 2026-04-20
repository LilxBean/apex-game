using UnityEngine;

namespace APEX.Meta
{
    /// <summary>
    /// Placeholder base for run modifiers (Challenge / Endless configs). No CreateAssetMenu
    /// yet — concrete subclasses will add their own entries once the modifier surface is defined.
    /// </summary>
    public abstract class RunModifier : ScriptableObject
    {
    }
}
