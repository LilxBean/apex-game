using APEX.Combat;
using APEX.Core.Events;
using APEX.Enemies;
using APEX.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Top-of-screen HUD: level + two-tier XP fill, MM:SS timer, kill counter.
    /// Subscribes to EventBus and reads PlayerXP for the fill fraction.
    /// </summary>
    public class ProgressionHUD : MonoBehaviour
    {
        [SerializeField] private PlayerXP _playerXP;
        [SerializeField] private RunManager _runManager;

        private TextMeshProUGUI _levelText;
        private Image _bigBarFill;
        private Image _thinBarFill;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _killsText;

        private int _kills;

        public void Bind(PlayerXP xp, RunManager runManager)
        {
            _playerXP = xp;
            _runManager = runManager;
        }

        private void Awake() { BuildUI(); }

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += OnEnemyKilled;
            EventBus.OnXPGained += OnXPGained;
            EventBus.OnLevelUp += OnLevelUp;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= OnEnemyKilled;
            EventBus.OnXPGained -= OnXPGained;
            EventBus.OnLevelUp -= OnLevelUp;
        }

        private void Update()
        {
            if (_timerText != null && _runManager != null)
            {
                float t = _runManager.RunTime;
                int mm = Mathf.FloorToInt(t / 60f);
                int ss = Mathf.FloorToInt(t % 60f);
                _timerText.text = $"{mm:00}:{ss:00}";
            }
        }

        private void OnEnemyKilled(EnemyController _, Damage __)
        {
            _kills++;
            if (_killsText != null) _killsText.text = $"Kills: {_kills}";
        }

        private void OnXPGained(int _, int __, int ___, float fraction)
        {
            if (_bigBarFill != null) _bigBarFill.fillAmount = fraction;
            if (_thinBarFill != null) _thinBarFill.fillAmount = fraction;
        }

        private void OnLevelUp(int newLevel)
        {
            if (_levelText != null) _levelText.text = $"LV {newLevel}";
            if (_bigBarFill != null) _bigBarFill.fillAmount = _playerXP != null ? _playerXP.LevelFraction : 0f;
            if (_thinBarFill != null) _thinBarFill.fillAmount = _bigBarFill != null ? _bigBarFill.fillAmount : 0f;
        }

        private void BuildUI()
        {
            var canvas = UIFactory.CreateOverlayCanvas(transform, "HUD_Canvas", sortingOrder: 10);

            // XP bar container at the top, spans width.
            var barWrap = new GameObject("XPBarWrap", typeof(RectTransform));
            barWrap.transform.SetParent(canvas.transform, false);
            var wrapRt = (RectTransform)barWrap.transform;
            wrapRt.anchorMin = new Vector2(0f, 1f);
            wrapRt.anchorMax = new Vector2(1f, 1f);
            wrapRt.pivot = new Vector2(0.5f, 1f);
            wrapRt.sizeDelta = new Vector2(0f, 80f);
            wrapRt.anchoredPosition = new Vector2(0f, 0f);

            // Big bar background.
            var bigBg = new GameObject("BigBar_BG", typeof(RectTransform), typeof(Image));
            bigBg.transform.SetParent(barWrap.transform, false);
            var bigBgRt = (RectTransform)bigBg.transform;
            bigBgRt.anchorMin = new Vector2(0f, 0.35f);
            bigBgRt.anchorMax = new Vector2(1f, 0.95f);
            bigBgRt.offsetMin = new Vector2(24f, 0f);
            bigBgRt.offsetMax = new Vector2(-24f, 0f);
            bigBg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            // Big bar fill (horizontal).
            var bigFill = new GameObject("BigBar_Fill", typeof(RectTransform), typeof(Image));
            bigFill.transform.SetParent(bigBg.transform, false);
            var bigFillRt = (RectTransform)bigFill.transform;
            bigFillRt.anchorMin = Vector2.zero;
            bigFillRt.anchorMax = Vector2.one;
            bigFillRt.offsetMin = new Vector2(4f, 4f);
            bigFillRt.offsetMax = new Vector2(-4f, -4f);
            _bigBarFill = bigFill.GetComponent<Image>();
            _bigBarFill.color = new Color(0.35f, 0.85f, 0.45f, 1f);
            _bigBarFill.type = Image.Type.Filled;
            _bigBarFill.fillMethod = Image.FillMethod.Horizontal;
            _bigBarFill.fillOrigin = 0;
            _bigBarFill.fillAmount = 0f;

            // Level label overlays big bar, left side.
            _levelText = UIFactory.CreateText(bigBg.transform, "LV 1", 32f, TextAlignmentOptions.MidlineLeft);
            _levelText.fontStyle = FontStyles.Bold;
            var lvlRt = _levelText.rectTransform;
            lvlRt.anchorMin = new Vector2(0f, 0f);
            lvlRt.anchorMax = new Vector2(1f, 1f);
            lvlRt.offsetMin = new Vector2(16f, 0f);
            lvlRt.offsetMax = new Vector2(-16f, 0f);

            // Thin bar below.
            var thinBg = new GameObject("ThinBar_BG", typeof(RectTransform), typeof(Image));
            thinBg.transform.SetParent(barWrap.transform, false);
            var thinBgRt = (RectTransform)thinBg.transform;
            thinBgRt.anchorMin = new Vector2(0f, 0.08f);
            thinBgRt.anchorMax = new Vector2(1f, 0.30f);
            thinBgRt.offsetMin = new Vector2(24f, 0f);
            thinBgRt.offsetMax = new Vector2(-24f, 0f);
            thinBg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

            var thinFill = new GameObject("ThinBar_Fill", typeof(RectTransform), typeof(Image));
            thinFill.transform.SetParent(thinBg.transform, false);
            var thinFillRt = (RectTransform)thinFill.transform;
            thinFillRt.anchorMin = Vector2.zero;
            thinFillRt.anchorMax = Vector2.one;
            thinFillRt.offsetMin = new Vector2(2f, 2f);
            thinFillRt.offsetMax = new Vector2(-2f, -2f);
            _thinBarFill = thinFill.GetComponent<Image>();
            _thinBarFill.color = new Color(0.75f, 0.95f, 0.55f, 1f);
            _thinBarFill.type = Image.Type.Filled;
            _thinBarFill.fillMethod = Image.FillMethod.Horizontal;
            _thinBarFill.fillOrigin = 0;
            _thinBarFill.fillAmount = 0f;

            // Timer (top-right).
            _timerText = UIFactory.CreateText(canvas.transform, "00:00", 32f, TextAlignmentOptions.TopRight);
            var tmRt = _timerText.rectTransform;
            tmRt.anchorMin = new Vector2(1f, 1f);
            tmRt.anchorMax = new Vector2(1f, 1f);
            tmRt.pivot = new Vector2(1f, 1f);
            tmRt.sizeDelta = new Vector2(240f, 60f);
            tmRt.anchoredPosition = new Vector2(-24f, -96f);

            // Kills (top-left).
            _killsText = UIFactory.CreateText(canvas.transform, "Kills: 0", 28f, TextAlignmentOptions.TopLeft);
            var kRt = _killsText.rectTransform;
            kRt.anchorMin = new Vector2(0f, 1f);
            kRt.anchorMax = new Vector2(0f, 1f);
            kRt.pivot = new Vector2(0f, 1f);
            kRt.sizeDelta = new Vector2(240f, 48f);
            kRt.anchoredPosition = new Vector2(24f, -96f);
        }
    }
}
