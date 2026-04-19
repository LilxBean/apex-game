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
        [SerializeField] private Health _playerHealth;

        private TextMeshProUGUI _levelText;
        private Image _bigBarFill;
        private Image _thinBarFill;
        private Image _hpBarFill;
        private TextMeshProUGUI _hpText;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _killsText;

        private int _kills;

        public void Bind(PlayerXP xp, RunManager runManager, Health playerHealth = null)
        {
            _playerXP = xp;
            _runManager = runManager;
            if (playerHealth != null) _playerHealth = playerHealth;
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

            if (_playerHealth != null)
            {
                float max = Mathf.Max(1f, _playerHealth.MaxHp);
                float cur = Mathf.Max(0f, _playerHealth.CurrentHp);
                if (_hpBarFill != null) _hpBarFill.fillAmount = cur / max;
                if (_hpText != null) _hpText.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}";
            }

            // Thin bar trails the big bar so XP pickups visibly "flow in".
            // On level-up the big bar drops sharply, so snap the thin bar down in that case
            // rather than letting it drain backward.
            if (_bigBarFill != null && _thinBarFill != null)
            {
                float target = _bigBarFill.fillAmount;
                float current = _thinBarFill.fillAmount;
                if (target < current)
                {
                    _thinBarFill.fillAmount = target;
                }
                else if (target > current)
                {
                    const float fillSpeedPerSecond = 1.6f;
                    _thinBarFill.fillAmount = Mathf.MoveTowards(current, target, fillSpeedPerSecond * Time.unscaledDeltaTime);
                }
            }
        }

        private void OnEnemyKilled(EnemyController _, Damage __)
        {
            _kills++;
            if (_killsText != null) _killsText.text = $"Kills: {_kills}";
        }

        private void OnXPGained(int _, int __, int ___, float fraction)
        {
            // Big bar snaps to the authoritative value; thin bar trails in Update.
            if (_bigBarFill != null) _bigBarFill.fillAmount = Mathf.Clamp01(fraction);
        }

        private void OnLevelUp(int newLevel)
        {
            if (_levelText != null) _levelText.text = $"LV {newLevel}";
            // Big bar snaps to post-cascade fraction; Update snaps the thin bar down to match
            // next frame (so it can't visually "drain backward" on level-up).
            if (_bigBarFill != null) _bigBarFill.fillAmount = _playerXP != null ? _playerXP.LevelFraction : 0f;
        }

        private void BuildUI()
        {
            var canvas = UIFactory.CreateOverlayCanvas(transform, "HUD_Canvas", sortingOrder: 10);

            // XP/HP bar container at the top, spans width.
            var barWrap = new GameObject("XPBarWrap", typeof(RectTransform));
            barWrap.transform.SetParent(canvas.transform, false);
            var wrapRt = (RectTransform)barWrap.transform;
            wrapRt.anchorMin = new Vector2(0f, 1f);
            wrapRt.anchorMax = new Vector2(1f, 1f);
            wrapRt.pivot = new Vector2(0.5f, 1f);
            wrapRt.sizeDelta = new Vector2(0f, 120f);
            wrapRt.anchoredPosition = new Vector2(0f, 0f);

            // Big bar background.
            var bigBg = new GameObject("BigBar_BG", typeof(RectTransform), typeof(Image));
            bigBg.transform.SetParent(barWrap.transform, false);
            var bigBgRt = (RectTransform)bigBg.transform;
            bigBgRt.anchorMin = new Vector2(0f, 0.60f);
            bigBgRt.anchorMax = new Vector2(1f, 0.96f);
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
            _bigBarFill.sprite = UIFactory.GetWhiteSprite();
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
            thinBgRt.anchorMin = new Vector2(0f, 0.42f);
            thinBgRt.anchorMax = new Vector2(1f, 0.56f);
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
            _thinBarFill.sprite = UIFactory.GetWhiteSprite();
            _thinBarFill.color = new Color(0.98f, 0.82f, 0.25f, 1f);
            _thinBarFill.type = Image.Type.Filled;
            _thinBarFill.fillMethod = Image.FillMethod.Horizontal;
            _thinBarFill.fillOrigin = 0;
            _thinBarFill.fillAmount = 0f;

            // HP bar below the XP bars.
            var hpBg = new GameObject("HPBar_BG", typeof(RectTransform), typeof(Image));
            hpBg.transform.SetParent(barWrap.transform, false);
            var hpBgRt = (RectTransform)hpBg.transform;
            hpBgRt.anchorMin = new Vector2(0f, 0.06f);
            hpBgRt.anchorMax = new Vector2(1f, 0.34f);
            hpBgRt.offsetMin = new Vector2(24f, 0f);
            hpBgRt.offsetMax = new Vector2(-24f, 0f);
            hpBg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var hpFill = new GameObject("HPBar_Fill", typeof(RectTransform), typeof(Image));
            hpFill.transform.SetParent(hpBg.transform, false);
            var hpFillRt = (RectTransform)hpFill.transform;
            hpFillRt.anchorMin = Vector2.zero;
            hpFillRt.anchorMax = Vector2.one;
            hpFillRt.offsetMin = new Vector2(3f, 3f);
            hpFillRt.offsetMax = new Vector2(-3f, -3f);
            _hpBarFill = hpFill.GetComponent<Image>();
            _hpBarFill.sprite = UIFactory.GetWhiteSprite();
            _hpBarFill.color = new Color(0.85f, 0.25f, 0.25f, 1f);
            _hpBarFill.type = Image.Type.Filled;
            _hpBarFill.fillMethod = Image.FillMethod.Horizontal;
            _hpBarFill.fillOrigin = 0;
            _hpBarFill.fillAmount = 1f;

            _hpText = UIFactory.CreateText(hpBg.transform, "0 / 0", 22f, TextAlignmentOptions.Center);
            _hpText.fontStyle = FontStyles.Bold;
            var hpTextRt = _hpText.rectTransform;
            hpTextRt.anchorMin = Vector2.zero;
            hpTextRt.anchorMax = Vector2.one;
            hpTextRt.offsetMin = new Vector2(12f, 0f);
            hpTextRt.offsetMax = new Vector2(-12f, 0f);

            // Timer (top-right).
            _timerText = UIFactory.CreateText(canvas.transform, "00:00", 32f, TextAlignmentOptions.TopRight);
            var tmRt = _timerText.rectTransform;
            tmRt.anchorMin = new Vector2(1f, 1f);
            tmRt.anchorMax = new Vector2(1f, 1f);
            tmRt.pivot = new Vector2(1f, 1f);
            tmRt.sizeDelta = new Vector2(240f, 60f);
            tmRt.anchoredPosition = new Vector2(-24f, -136f);

            // Kills (top-left).
            _killsText = UIFactory.CreateText(canvas.transform, "Kills: 0", 28f, TextAlignmentOptions.TopLeft);
            var kRt = _killsText.rectTransform;
            kRt.anchorMin = new Vector2(0f, 1f);
            kRt.anchorMax = new Vector2(0f, 1f);
            kRt.pivot = new Vector2(0f, 1f);
            kRt.sizeDelta = new Vector2(240f, 48f);
            kRt.anchoredPosition = new Vector2(24f, -136f);
        }
    }
}
