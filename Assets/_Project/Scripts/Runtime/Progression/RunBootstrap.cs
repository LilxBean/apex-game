using APEX.CameraRig;
using APEX.Enemies.Spawning;
using APEX.Meta;
using APEX.Player;
using APEX.UI;
using APEX.World;
using UnityEngine;

namespace APEX.Progression
{
    /// <summary>
    /// Single scene-level object that wires up the Run lifecycle: creates the XP orb spawner,
    /// HUD canvas, level-up screen, run-end screen, and RunManager, and binds them to the
    /// player's PlayerXP / PlayerBuild. Add one of these to the Run scene.
    /// </summary>
    public class RunBootstrap : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PlayerController _player;

        [Header("Progression Data")]
        [SerializeField] private XPCurve _xpCurve;
        [SerializeField] private PickTable _pickTable;
        [SerializeField] private PickPool _pickPool;

        [Header("Prefabs")]
        [SerializeField] private GameObject _xpOrbPrefab;

        [Header("Run Tuning")]
        [SerializeField] private float _runLengthSeconds = 15f * 60f;

        [Header("World")]
        [SerializeField] private Vector2 _arenaSize = new(60f, 40f);
        [SerializeField] private float _cameraSmoothTime = 0.18f;

        // Wiring runs in Start rather than Awake so every scene component's Awake (notably
        // PlayerController.Awake, which sets _health via GetComponent and calls Health.Initialize)
        // has completed — otherwise _player.Health can observe null and the HUD binds to null.
        private void Start()
        {
            var request = SceneLoader.ConsumePendingRequest();
            if (request == null)
            {
                Debug.LogWarning("[APEX] Run scene started without RunRequest, using defaults.");
            }
            else if (request.Mode != Mode.Main)
            {
                Debug.LogWarning($"[APEX] Mode {request.Mode} not implemented this session, falling back to Main.");
            }

            if (_player == null)
            {
                _player = FindFirstObjectByType<PlayerController>();
            }
            if (_player == null)
            {
                Debug.LogError("[RunBootstrap] No PlayerController found.");
                enabled = false;
                return;
            }

            var playerGo = _player.gameObject;

            // Arena bounds + background first so camera can clamp to them.
            var arenaGo = new GameObject("ArenaBounds");
            arenaGo.transform.SetParent(transform);
            var arena = arenaGo.AddComponent<ArenaBounds>();
            arena.Build(Vector2.zero, _arenaSize);

            var bgGo = new GameObject("Background", typeof(SpriteRenderer));
            bgGo.transform.SetParent(transform);
            var bg = bgGo.AddComponent<BackgroundTiler>();
            bg.Build(arena.WorldRect);

            // Camera follow on the Run scene's camera. We must not use Camera.main directly —
            // during the additive transition MainMenu may still be loaded and its tagged
            // MainCamera would be returned, then destroyed on unload, leaving no follow.
            var mainCam = FindCameraInOwnScene();
            if (mainCam != null)
            {
                var follow = mainCam.GetComponent<CameraFollow2D>();
                if (follow == null) follow = mainCam.gameObject.AddComponent<CameraFollow2D>();
                follow.Configure(_player.transform, _player.Rigidbody, _cameraSmoothTime);
                follow.SetClamp(arena.WorldRect);
                follow.ConfigureShakeTarget(_player.Health);
                follow.SnapToTarget();
            }
            else
            {
                Debug.LogWarning("[RunBootstrap] No Camera found in Run scene; skipping follow camera.");
            }

            // Clamp enemy spawns to the arena.
            var enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if (enemySpawner != null) enemySpawner.SetArenaBounds(arena.WorldRect);

            var xp = playerGo.GetComponent<PlayerXP>();
            if (xp == null) xp = playerGo.AddComponent<PlayerXP>();
            if (xp.Curve == null) xp.Curve = _xpCurve;

            var build = playerGo.GetComponent<PlayerBuild>();
            if (build == null) build = playerGo.AddComponent<PlayerBuild>();

            // XP orb spawner.
            var spawnerGo = new GameObject("XPOrbSpawner");
            spawnerGo.transform.SetParent(transform);
            var spawner = spawnerGo.AddComponent<XPOrbSpawner>();
            SerializedAssign(spawner, "_orbPrefab", _xpOrbPrefab);
            SerializedAssign(spawner, "_playerTransform", _player.transform);
            SerializedAssign(spawner, "_playerXP", xp);

            // Run manager.
            var runMgrGo = new GameObject("RunManager");
            runMgrGo.transform.SetParent(transform);
            var runManager = runMgrGo.AddComponent<RunManager>();
            SerializedAssign(runManager, "_runLengthSeconds", _runLengthSeconds);
            runManager.Bind(xp, _player.Health);

            // HUD.
            var hudGo = new GameObject("ProgressionHUD");
            hudGo.transform.SetParent(transform);
            var hud = hudGo.AddComponent<ProgressionHUD>();
            hud.Bind(xp, runManager, _player.Health);

            // Level-up screen.
            var levelUpGo = new GameObject("LevelUpScreen");
            levelUpGo.transform.SetParent(transform);
            var levelUp = levelUpGo.AddComponent<LevelUpScreen>();
            levelUp.Bind(_pickTable, _pickPool, build);

            // Run-end screen.
            var endGo = new GameObject("RunEndScreen");
            endGo.transform.SetParent(transform);
            var endScreen = endGo.AddComponent<RunEndScreen>();

            // Pause overlay (Escape).
            var pauseGo = new GameObject("PauseController");
            pauseGo.transform.SetParent(transform);
            var pause = pauseGo.AddComponent<PauseController>();
            pause.Bind(runManager, endScreen, levelUp);

            // Floating damage numbers.
            var dmgGo = new GameObject("FloatingDamageNumbers");
            dmgGo.transform.SetParent(transform);
            dmgGo.AddComponent<FloatingDamageNumbers>();
        }

        private Camera FindCameraInOwnScene()
        {
            var ownScene = gameObject.scene;
            var all = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Camera fallback = null;
            foreach (var cam in all)
            {
                if (cam.gameObject.scene == ownScene)
                {
                    if (cam.CompareTag("MainCamera")) return cam;
                    fallback ??= cam;
                }
            }
            return fallback;
        }

        // Helper: set a private serialized field by reflection (safe at runtime; Bind methods
        // cover the common cases but some components take references via [SerializeField]-only).
        private static void SerializedAssign(Object target, string field, object value)
        {
            var t = target.GetType();
            var f = t.GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (f != null) f.SetValue(target, value);
        }
    }
}
