using UnityEngine;

namespace APEX.Core
{
    /// <summary>
    /// Shared run tuning asset. Consolidates values that previously lived as hard-coded
    /// fields on multiple scene components (RunManager, RunBootstrap).
    /// </summary>
    [CreateAssetMenu(fileName = "RunConfig", menuName = "APEX/Core/Run Config")]
    public class RunConfig : ScriptableObject
    {
        public float RunLengthSeconds = 300f;
    }
}
