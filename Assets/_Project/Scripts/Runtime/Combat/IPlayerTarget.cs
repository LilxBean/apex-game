using UnityEngine;

namespace APEX.Combat
{
    /// <summary>
    /// Contract that AI behaviours use to locate the player.
    /// </summary>
    public interface IPlayerTarget
    {
        Vector2 Position { get; }
    }
}
