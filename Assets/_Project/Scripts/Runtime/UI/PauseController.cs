using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace APEX.UI
{
    /// <summary>
    /// Toggles Time.timeScale on Escape and shows a translucent "Paused" overlay.
    /// Ignores the key while another system already has timeScale at 0 (LevelUpScreen
    /// during a pick, or the game over flow after the player dies), so pause can't
    /// hijack those modals.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        private GameObject _overlay;
        private bool _isPaused;

        private void Awake()
        {
            BuildOverlay();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (!keyboard.escapeKey.wasPressedThisFrame) return;

            // Don't toggle if another modal has already paused the sim.
            if (!_isPaused && Time.timeScale == 0f) return;

            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;
            if (_overlay != null) _overlay.SetActive(_isPaused);
        }

        private void OnDisable()
        {
            // Defensive: never leave the game frozen if this object is torn down while paused.
            if (_isPaused)
            {
                Time.timeScale = 1f;
                _isPaused = false;
            }
        }

        private void BuildOverlay()
        {
            // Sort above HUD (10) but below LevelUpScreen (100) / RunEndScreen (150).
            var canvas = UIFactory.CreateOverlayCanvas(transform, "PauseOverlay_Canvas", sortingOrder: 50);
            _overlay = canvas;

            UIFactory.CreateFullscreenPanel(canvas.transform, new Color(0f, 0f, 0f, 0.55f));

            var label = UIFactory.CreateText(canvas.transform, "Paused", 96f, TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            var lr = label.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = Vector2.zero;

            canvas.SetActive(false);
        }
    }
}
