using APEX.Core;
using APEX.Core.Events;
using APEX.Meta;
using APEX.Progression;
using UnityEngine;
using UnityEngine.InputSystem;

namespace APEX.UI
{
    /// <summary>
    /// Owns the in-run pause state. Listens for the Gameplay/Pause action (Escape / Start),
    /// spawns the PauseScreen overlay, and routes its Resume/Settings/Restart/EndRun/Quit
    /// callbacks. Suppressed while LevelUpScreen or RunEndScreen is open — those modals own
    /// input focus and pause should not layer on top of them.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        private APEXControls _controls;
        private InputAction _pauseAction;

        private RunManager _runManager;
        private RunEndScreen _runEndScreen;
        private LevelUpScreen _levelUpScreen;

        private GameObject _canvas;
        private PauseScreen _pauseScreen;
        private SettingsScreen _settingsScreen;

        private bool _isPaused;
        private float _priorTimeScale = 1f;

        public bool IsPaused => _isPaused;

        public void Bind(RunManager runManager, RunEndScreen runEndScreen, LevelUpScreen levelUpScreen)
        {
            _runManager = runManager;
            _runEndScreen = runEndScreen;
            _levelUpScreen = levelUpScreen;
        }

        private void Awake()
        {
            // Sits above HUD (10) but below LevelUpScreen (100) / RunEndScreen (150).
            _canvas = UIFactory.CreateOverlayCanvas(transform, "PauseOverlay_Canvas", sortingOrder: 50);

            _pauseScreen = gameObject.AddComponent<PauseScreen>();
            _pauseScreen.Init(_canvas.transform, new PauseScreen.Callbacks
            {
                OnResume = Resume,
                OnSettings = OpenSettings,
                OnRestart = OnRestart,
                OnEndRun = OnEndRun,
                OnQuitToMenu = OnQuitToMenu
            });
            _pauseScreen.Hide();

            _controls = new APEXControls();
            _pauseAction = _controls.Gameplay.Pause;
            _pauseAction.performed += OnPausePerformed;
        }

        private void OnEnable()
        {
            _controls?.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _controls?.Gameplay.Disable();
            // Defensive: never leave the game frozen if this component is disabled while paused.
            if (_isPaused)
            {
                Time.timeScale = _priorTimeScale <= 0f ? 1f : _priorTimeScale;
                _isPaused = false;
            }
        }

        private void OnDestroy()
        {
            if (_pauseAction != null) _pauseAction.performed -= OnPausePerformed;
            _controls?.Dispose();
        }

        private void OnPausePerformed(InputAction.CallbackContext _)
        {
            // Escape is swallowed whenever a higher-priority modal owns the run.
            if (_isPaused) { Resume(); return; }
            if (IsBlockedByModal()) return;
            Pause();
        }

        private bool IsBlockedByModal()
        {
            if (_levelUpScreen != null && _levelUpScreen.IsOpen) return true;
            if (_runEndScreen != null && _runEndScreen.IsOpen) return true;
            return false;
        }

        public void Pause()
        {
            if (_isPaused) return;
            if (IsBlockedByModal()) return;

            _priorTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
            _isPaused = true;
            Time.timeScale = 0f;

            CloseSettingsIfOpen();
            _pauseScreen.Show();
            EventBus.RaisePaused();
        }

        public void Resume()
        {
            if (!_isPaused) return;

            CloseSettingsIfOpen();
            _pauseScreen.Hide();

            Time.timeScale = _priorTimeScale <= 0f ? 1f : _priorTimeScale;
            _isPaused = false;
            EventBus.RaiseResumed();
        }

        private void OpenSettings()
        {
            _pauseScreen.Hide();
            _settingsScreen = gameObject.AddComponent<SettingsScreen>();
            _settingsScreen.Init(_canvas.transform, OnSettingsClosed);
        }

        private void OnSettingsClosed()
        {
            CloseSettingsIfOpen();
            if (_isPaused) _pauseScreen.Show();
        }

        private void CloseSettingsIfOpen()
        {
            if (_settingsScreen != null)
            {
                Destroy(_settingsScreen);
                _settingsScreen = null;
            }
        }

        private void OnRestart()
        {
            Resume();
            SceneLoader.Run(SceneLoader.RestartRun());
        }

        private void OnEndRun()
        {
            Resume();
            if (_runManager != null)
            {
                _runManager.EndRun(EndReason.Quit);
            }
            else if (_runEndScreen != null)
            {
                // Degenerate path — no RunManager bound. Surface the screen with empty stats.
                _runEndScreen.Show(default, EndReason.Quit);
            }
        }

        private void OnQuitToMenu()
        {
            Resume();
            // Fire OnRunEnded for future stats subscribers, but don't flash the end screen
            // while the scene is transitioning out.
            if (_runEndScreen != null) _runEndScreen.SuppressNextShow();
            if (_runManager != null) _runManager.EndRun(EndReason.Quit);
            // RunManager.EndRun sets timeScale=0 for the (suppressed) end-screen path; undo
            // so the Main Menu scene doesn't inherit a frozen clock.
            Time.timeScale = 1f;
            SceneLoader.Run(SceneLoader.ReturnToMainMenu());
        }
    }
}
