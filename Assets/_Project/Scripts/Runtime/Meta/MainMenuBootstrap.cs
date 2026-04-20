using APEX.UI;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.Meta
{
    /// <summary>
    /// Sits on an empty GameObject in MainMenu.unity. Ensures the persistent services
    /// singleton exists (SettingsService, DontDestroyOnLoad) and spawns MainMenuScreen
    /// under the scene's first ScreenSpaceOverlay Canvas.
    /// </summary>
    public class MainMenuBootstrap : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        private void Awake()
        {
            EnsurePersistentServices();
        }

        private void Start()
        {
            var canvas = _canvas != null ? _canvas : FindCanvasInOwnScene();
            if (canvas == null)
            {
                Debug.LogError("[APEX] MainMenuBootstrap: no Canvas found in MainMenu scene.");
                return;
            }

            ConfigureCanvas(canvas);

            var menuGo = new GameObject("MainMenuScreen");
            menuGo.transform.SetParent(transform);
            var screen = menuGo.AddComponent<MainMenuScreen>();
            screen.Build(canvas.transform);
        }

        // During the additive transition both MainMenu and Run can be loaded at the same time.
        // FindFirstObjectByType would return either scene's Canvas, so scope the lookup to the
        // scene this bootstrap lives in.
        private Canvas FindCanvasInOwnScene()
        {
            var ownScene = gameObject.scene;
            var all = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in all)
            {
                if (c.gameObject.scene == ownScene) return c;
            }
            return null;
        }

        // Force the expected config regardless of how the user authored the scene — keeps
        // layout math in MainMenuScreen honest across different Canvas default states.
        private static void ConfigureCanvas(Canvas canvas)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static void EnsurePersistentServices()
        {
            if (SettingsService.Instance != null) return;

            var root = new GameObject("APEX_PersistentServices");
            root.AddComponent<SettingsService>();
            DontDestroyOnLoad(root);
        }
    }
}
