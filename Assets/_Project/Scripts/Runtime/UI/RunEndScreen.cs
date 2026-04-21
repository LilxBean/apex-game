using System.Globalization;
using APEX.Core;
using APEX.Core.Events;
using APEX.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Canvas overlay shown on OnRunEnded. Renders the full RunStats payload — title and
    /// color branch on EndReason. Exposes Play Again / Main Menu buttons.
    /// </summary>
    public class RunEndScreen : MonoBehaviour
    {
        private static readonly Color VictoryColor = new(1f, 0.847f, 0.420f, 1f); // #FFD86B
        private static readonly Color DefeatColor = new(0.816f, 0.251f, 0.251f, 1f); // #D04040
        private static readonly Color QuitColor = new(0.66f, 0.66f, 0.66f, 1f);

        private GameObject _root;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _labelsText;
        private TextMeshProUGUI _valuesText;
        private bool _isOpen;
        private bool _suppressNext;

        public bool IsOpen => _isOpen;

        /// <summary>
        /// Swallow the next OnRunEnded without rendering. Used by Quit-to-Main-Menu: we want
        /// the event to reach future subscribers (StatsRecorder) without flashing the end
        /// screen while the scene transitions.
        /// </summary>
        public void SuppressNextShow() { _suppressNext = true; }

        private void Awake() { BuildUI(); Hide(); }

        private void OnEnable() { EventBus.OnRunEnded += OnRunEnded; }
        private void OnDisable() { EventBus.OnRunEnded -= OnRunEnded; }

        private void Hide()
        {
            if (_root != null) _root.SetActive(false);
            _isOpen = false;
        }

        private void OnRunEnded(RunStats stats, EndReason reason)
        {
            if (_suppressNext)
            {
                _suppressNext = false;
                return;
            }
            Show(stats, reason);
        }

        /// <summary>Public entry point so non-event callers (e.g. Pause → End Run) share the death-path render.</summary>
        public void Show(RunStats stats, EndReason reason)
        {
            if (_titleText != null)
            {
                _titleText.text = TitleFor(reason);
                _titleText.color = ColorFor(reason);
            }

            if (_labelsText != null && _valuesText != null)
            {
                int mm = Mathf.FloorToInt(stats.RunDurationSeconds / 60f);
                int ss = Mathf.FloorToInt(stats.RunDurationSeconds % 60f);

                var ci = CultureInfo.InvariantCulture;
                _labelsText.text = string.Join("\n",
                    "Time Survived",
                    "Level Reached",
                    "Kills",
                    "Damage Dealt",
                    "Avg DPS",
                    "Peak DPS",
                    "XP Gained",
                    "Picks");
                _valuesText.text = string.Join("\n",
                    $"{mm:00}:{ss:00}",
                    stats.LevelReached.ToString(ci),
                    stats.Kills.ToString("N0", ci),
                    Mathf.RoundToInt(stats.DamageDealt).ToString("N0", ci),
                    stats.AverageDPS.ToString("N1", ci),
                    stats.PeakDPS.ToString("N1", ci),
                    stats.XPTotal.ToString("N0", ci),
                    FormatPicks(stats));
            }

            if (_root != null) _root.SetActive(true);
            _isOpen = true;
        }

        private static string FormatPicks(RunStats stats)
        {
            if (stats.PicksTaken == 0) return "0";
            return $"{stats.PicksTaken}   ({stats.PassivesTaken} passive, {stats.HammersTaken} hammer)";
        }

        private static string TitleFor(EndReason reason) => reason switch
        {
            EndReason.Victory => "VICTORY",
            EndReason.Defeat => "DEFEAT",
            _ => "RUN ENDED"
        };

        private static Color ColorFor(EndReason reason) => reason switch
        {
            EndReason.Victory => VictoryColor,
            EndReason.Defeat => DefeatColor,
            _ => QuitColor
        };

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

            _titleText = UIFactory.CreateText(dim.transform, "RUN ENDED", 72, TextAlignmentOptions.Center);
            _titleText.fontStyle = FontStyles.Bold;
            var tr = _titleText.rectTransform;
            tr.anchorMin = new Vector2(0f, 0.78f);
            tr.anchorMax = new Vector2(1f, 0.92f);
            tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;

            _labelsText = UIFactory.CreateText(dim.transform, "", 28, TextAlignmentOptions.TopRight);
            _labelsText.color = new Color(1f, 1f, 1f, 0.72f);
            var lr = _labelsText.rectTransform;
            lr.anchorMin = new Vector2(0.20f, 0.32f);
            lr.anchorMax = new Vector2(0.50f, 0.76f);
            lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;

            _valuesText = UIFactory.CreateText(dim.transform, "", 28, TextAlignmentOptions.TopLeft);
            var vr = _valuesText.rectTransform;
            vr.anchorMin = new Vector2(0.52f, 0.32f);
            vr.anchorMax = new Vector2(0.82f, 0.76f);
            vr.offsetMin = Vector2.zero; vr.offsetMax = Vector2.zero;

            var restart = UIFactory.CreateButton(dim.transform, "Play Again");
            var rr = (RectTransform)restart.transform;
            rr.anchorMin = new Vector2(0.5f, 0.14f);
            rr.anchorMax = new Vector2(0.5f, 0.14f);
            rr.pivot = new Vector2(0.5f, 0.5f);
            rr.sizeDelta = new Vector2(320f, 72f);
            rr.anchoredPosition = new Vector2(-180f, 0f);
            restart.onClick.AddListener(OnRestartClicked);

            var menu = UIFactory.CreateButton(dim.transform, "Main Menu");
            var mr = (RectTransform)menu.transform;
            mr.anchorMin = new Vector2(0.5f, 0.14f);
            mr.anchorMax = new Vector2(0.5f, 0.14f);
            mr.pivot = new Vector2(0.5f, 0.5f);
            mr.sizeDelta = new Vector2(320f, 72f);
            mr.anchoredPosition = new Vector2(180f, 0f);
            menu.onClick.AddListener(OnMainMenuClicked);
        }
    }
}
