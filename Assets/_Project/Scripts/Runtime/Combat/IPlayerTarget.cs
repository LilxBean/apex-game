using UnityEngine;

namespace APEX.Combat
{
    /// <summary>
    /// Contract that AI behaviours use to locate the player.
    /// Both the temp stub and the real player implement this.
    /// </summary>
    public interface IPlayerTarget
    {
        Vector2 Position { get; }
    }
}
