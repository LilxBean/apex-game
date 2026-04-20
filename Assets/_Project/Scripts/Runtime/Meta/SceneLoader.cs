using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace APEX.Meta
{
    /// <summary>
    /// Orchestrates additive scene transitions between MainMenu and Run. Keeps the request
    /// that started the current run in LastRequest so restart can reuse it.
    ///
    /// Transitions are run on a persistent DontDestroyOnLoad host so the coroutine survives
    /// the unload of the scene that initiated it. Callers use <see cref="Run"/> instead of
    /// hosting the coroutine themselves.
    /// </summary>
    public static class SceneLoader
    {
        public const string MainMenuSceneName = "MainMenu";
        public const string RunSceneName = "Run";

        public static RunRequest PendingRequest { get; private set; }
        public static RunRequest LastRequest { get; private set; }

        private static SceneLoaderHost _host;

        /// <summary>Returns PendingRequest and clears it. Callers should use this in Awake/Start.</summary>
        public static RunRequest ConsumePendingRequest()
        {
            var req = PendingRequest;
            PendingRequest = null;
            return req;
        }

        /// <summary>Runs <paramref name="routine"/> on a DontDestroyOnLoad host so it survives scene unloads.</summary>
        public static Coroutine Run(IEnumerator routine)
        {
            return GetHost().StartCoroutine(routine);
        }

        private static SceneLoaderHost GetHost()
        {
            if (_host != null) return _host;
            var go = new GameObject("APEX_SceneLoaderHost");
            Object.DontDestroyOnLoad(go);
            _host = go.AddComponent<SceneLoaderHost>();
            return _host;
        }

        private class SceneLoaderHost : MonoBehaviour { }

        public static IEnumerator StartRun(RunRequest req)
        {
            PendingRequest = req;
            LastRequest = req;

            var load = SceneManager.LoadSceneAsync(RunSceneName, LoadSceneMode.Additive);
            while (load is { isDone: false }) yield return null;

            var runScene = SceneManager.GetSceneByName(RunSceneName);
            if (runScene.IsValid()) SceneManager.SetActiveScene(runScene);

            var menuScene = SceneManager.GetSceneByName(MainMenuSceneName);
            if (menuScene.IsValid() && menuScene.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(menuScene);
                while (unload is { isDone: false }) yield return null;
            }
        }

        public static IEnumerator ReturnToMainMenu()
        {
            PendingRequest = null;
            LastRequest = null;

            var load = SceneManager.LoadSceneAsync(MainMenuSceneName, LoadSceneMode.Additive);
            while (load is { isDone: false }) yield return null;

            var menuScene = SceneManager.GetSceneByName(MainMenuSceneName);
            if (menuScene.IsValid()) SceneManager.SetActiveScene(menuScene);

            var runScene = SceneManager.GetSceneByName(RunSceneName);
            if (runScene.IsValid() && runScene.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(runScene);
                while (unload is { isDone: false }) yield return null;
            }
        }

        public static IEnumerator RestartRun()
        {
            if (LastRequest == null)
            {
                Debug.LogWarning("[APEX] RestartRun with no LastRequest, using Main defaults.");
                LastRequest = RunRequest.Default(Mode.Main);
            }

            PendingRequest = LastRequest;

            var runScene = SceneManager.GetSceneByName(RunSceneName);
            if (runScene.IsValid() && runScene.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(runScene);
                while (unload is { isDone: false }) yield return null;
            }

            var load = SceneManager.LoadSceneAsync(RunSceneName, LoadSceneMode.Additive);
            while (load is { isDone: false }) yield return null;

            var reloaded = SceneManager.GetSceneByName(RunSceneName);
            if (reloaded.IsValid()) SceneManager.SetActiveScene(reloaded);
        }
    }
}
