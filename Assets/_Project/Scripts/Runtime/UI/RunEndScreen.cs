using APEX.Core;
using APEX.Core.Events;
using APEX.Meta;
using APEX.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Canvas overlay shown on OnRunEnded. Renders the RunStats payload and exposes
    /// Restart (re-runs the same RunRequest via SceneLoader) and Main Menu (returns to
    /// MainMenu.unity) buttons.
    /// </summary>
    public class RunEndScreen : MonoBehaviour
    {
        [SerializeField] private RunManager _runManager;

        private GameObject _root;
        private TextMeshProUGUI _summary;

        public void Bind(RunManager runManager) { _runManager = runManager; }

        private void Awake() { BuildUI(); Hide(); }

        private void OnEnable() { EventBus.OnRunEnded += OnRunEnded; }
        private void OnDisable() { EventBus.OnRunEnded -= OnRunEnded; }

        private void Hide() { if (_root != null) _root.SetActive(false); }

        private void OnRunEnded(RunStats stats, EndReason reason)
        {
            if (_summary != null)
            {
                float t = stats.RunDurationSeconds;
                int mm = Mathf.FloorToInt(t / 60f);
                int ss = Mathf.FloorToInt(t % 60f);
                int kills = _runManager != null ? _runManager.Kills : 0;
                _summary.text =
                    $"Time Survived: {mm:00}:{ss:00}\n" +
                    $"Kills: {kills}\n" +
                    $"Level Reached: {stats.LevelReached}";
            }
            _root.SetActive(true);
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneLoader.Run(SceneLoader.RestartRun());
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneLoader.Run(SceneLoader.ReturnToMainMenu());
        }

        private void BuildUI()
        {
            _root = UIFactory.CreateOverlayCanvas(transform, "RunEndScreen_Canvas", sortingOrder: 150);
            var dim = UIFactory.CreateFullscreenPanel(_root.transform, new Color(0f, 0f, 0f, 0.85f));

            var title = UIFactory.CreateText(dim.transform, "Run Over", 64, TextAlignmentOptions.Center);
            var tr = title.rectTransform;
            tr.anchorMin = new Vector2(0f, 0.72f);
            tr.anchorMax = new Vector2(1f, 0.88f);
            tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;

            _summary = UIFactory.CreateText(dim.transform, "", 32, TextAlignmentOptions.Center);
            var sr = _summary.rectTransform;
            sr.anchorMin = new Vector2(0f, 0.40f);
            sr.anchorMax = new Vector2(1f, 0.68f);
            sr.offsetMin = Vector2.zero; sr.offsetMax = Vector2.zero;

            var restart = UIFactory.CreateButton(dim.transform, "Restart");
            var rr = (RectTransform)restart.transform;
            rr.anchorMin = new Vector2(0.5f, 0.18f);
            rr.anchorMax = new Vector2(0.5f, 0.18f);
            rr.pivot = new Vector2(0.5f, 0.5f);
            rr.sizeDelta = new Vector2(320f, 72f);
            rr.anchoredPosition = new Vector2(-180f, 0f);
            restart.onClick.AddListener(OnRestartClicked);

            var menu = UIFactory.CreateButton(dim.transform, "Main Menu");
            var mr = (RectTransform)menu.transform;
            mr.anchorMin = new Vector2(0.5f, 0.18f);
            mr.anchorMax = new Vector2(0.5f, 0.18f);
            mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(320f, 72f);
            mr.anchoredPosition = new Vector2(180f, 0f);
            menu.onClick.AddListener(OnMainMenuClicked);
        }
    }
}
