using APEX.Core.Events;
using APEX.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Canvas overlay shown on OnRunEnded. Displays time/kills/level and a Restart button
    /// that reloads the current scene.
    /// </summary>
    public class RunEndScreen : MonoBehaviour
    {
        [SerializeField] private RunManager _runManager;

        private GameObject _root;
        private TextMeshProUGUI _summary;

        public void Bind(RunManager runManager) { _runManager = runManager; }

        private void Awake() { BuildUI(); Hide(); }

        private void OnEnable() { EventBus.OnRunEnded += Show; }
        private void OnDisable() { EventBus.OnRunEnded -= Show; }

        private void Hide() { if (_root != null) _root.SetActive(false); }

        private void Show()
        {
            if (_runManager != null && _summary != null)
            {
                float t = _runManager.RunTime;
                int mm = Mathf.FloorToInt(t / 60f);
                int ss = Mathf.FloorToInt(t % 60f);
                _summary.text =
                    $"Time Survived: {mm:00}:{ss:00}\n" +
                    $"Kills: {_runManager.Kills}\n" +
                    $"Level Reached: {_runManager.LevelReached}";
            }
            _root.SetActive(true);
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

            var btn = UIFactory.CreateButton(dim.transform, "Restart");
            var br = (RectTransform)btn.transform;
            br.anchorMin = new Vector2(0.5f, 0.18f);
            br.anchorMax = new Vector2(0.5f, 0.18f);
            br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(320f, 72f);
            br.anchoredPosition = Vector2.zero;
            btn.onClick.AddListener(OnRestartClicked);
        }
    }
}
