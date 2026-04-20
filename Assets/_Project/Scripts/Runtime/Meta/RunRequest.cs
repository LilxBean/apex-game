using System.Collections.Generic;

namespace APEX.Meta
{
    public enum Mode
    {
        Main,
        Challenge,
        Endless
    }

    /// <summary>
    /// Description of a run to start. Passed from the menu layer to the run scene via
    /// SceneLoader.PendingRequest. Plain C# — not a MonoBehaviour or ScriptableObject.
    /// </summary>
    public class RunRequest
    {
        public Mode Mode;
        public string StartingEraId;          // null = default era
        public int Seed;                      // 0 = random
        public List<RunModifier> Modifiers = new();

        public static RunRequest Default(Mode mode)
        {
            return new RunRequest
            {
                Mode = mode,
                StartingEraId = null,
                Seed = 0,
                Modifiers = new List<RunModifier>()
            };
        }
    }
}
