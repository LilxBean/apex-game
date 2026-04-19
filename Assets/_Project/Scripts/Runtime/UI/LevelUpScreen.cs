using System.Collections.Generic;
using APEX.Core.Events;
using APEX.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Canvas overlay shown on OnLevelUp. Pauses the game, rolls 3 picks, applies the chosen one.
    /// UI is built programmatically at Awake so scenes don't need a hand-authored prefab.
    /// </summary>
    public class LevelUpScreen : MonoBehaviour
    {
        [SerializeField] private PickTable _pickTable;
        [SerializeField] private PickPool _pickPool;
        [SerializeField] private PlayerBuild _build;

        private const int CardCount = 3;

        private GameObject _root;
        private readonly Card[] _cards = new Card[CardCount];
        private readonly Queue<int> _pendingLevels = new();
        private bool _visible;

        public void Bind(PickTable table, PickPool pool, PlayerBuild build)
        {
            _pickTable = table;
            _pickPool = pool;
            _build = build;
        }

        private void Awake()
        {
            BuildUI();
            Hide();
        }

        private void OnEnable() { EventBus.OnLevelUp += OnLevelUp; }
        private void OnDisable() { EventBus.OnLevelUp -= OnLevelUp; }

        private void OnLevelUp(int level)
        {
            _pendingLevels.Enqueue(level);
            if (!_visible) ShowNext();
        }

        private void ShowNext()
        {
            if (_pendingLevels.Count == 0)
            {
                Hide();
                return;
            }
            int level = _pendingLevels.Dequeue();

            var picks = PickRoller.RollPicks(level, CardCount, _pickTable, _pickPool, _build);
            for (int i = 0; i < _cards.Length; i++)
            {
                if (i < picks.Count) _cards[i].Bind(picks[i], OnCardClicked);
                else _cards[i].Bind(null, OnCardClicked);
            }

            _root.SetActive(true);
            _visible = true;
            Time.timeScale = 0f;
        }

        private void Hide()
        {
            if (_root != null) _root.SetActive(false);
            _visible = false;
            // Only un-pause when the queue is fully drained.
            if (_pendingLevels.Count == 0) Time.timeScale = 1f;
        }

        private void OnCardClicked(PickRoller.PickOption? opt)
        {
            if (opt.HasValue)
            {
                if (opt.Value.IsHammer) _build.ApplyPick(opt.Value.hammer);
                else _build.ApplyPick(opt.Value.passive);
            }

            _root.SetActive(false);
            _visible = false;

            if (_pendingLevels.Count > 0) ShowNext();
            else Time.timeScale = 1f;
        }

        // --- UI construction -------------------------------------------------

        private void BuildUI()
        {
            _root = UIFactory.CreateOverlayCanvas(transform, "LevelUpScreen_Canvas", sortingOrder: 100);
            var dim = UIFactory.CreateFullscreenPanel(_root.transform, new Color(0f, 0f, 0f, 0.78f));

            var title = UIFactory.CreateText(dim.transform, "Level Up — Choose One", 48, TextAlignmentOptions.Center);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0.78f);
            titleRect.anchorMax = new Vector2(1f, 0.90f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            var row = new GameObject("CardRow", typeof(RectTransform));
            row.transform.SetParent(dim.transform, false);
            var rowRect = (RectTransform)row.transform;
            rowRect.anchorMin = new Vector2(0.5f, 0.25f);
            rowRect.anchorMax = new Vector2(0.5f, 0.72f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.sizeDelta = new Vector2(1200f, 0f);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            for (int i = 0; i < CardCount; i++)
            {
                _cards[i] = Card.Build(row.transform);
            }
        }

        // --- Card widget -----------------------------------------------------

        private class Card
        {
            public GameObject Root;
            public Image RarityStrip;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI TypeText;
            public TextMeshProUGUI DescText;
            public Button Button;

            private PickRoller.PickOption? _option;
            private System.Action<PickRoller.PickOption?> _onClick;

            public static Card Build(Transform parent)
            {
                var c = new Card();
                c.Root = new GameObject("Card", typeof(RectTransform), typeof(Image));
                c.Root.transform.SetParent(parent, false);
                var img = c.Root.GetComponent<Image>();
                img.color = new Color(0.12f, 0.12f, 0.14f, 1f);

                c.Button = c.Root.AddComponent<Button>();
                c.Button.targetGraphic = img;
                c.Button.onClick.AddListener(() => c._onClick?.Invoke(c._option));

                // Rarity strip at the top.
                var strip = new GameObject("RarityStrip", typeof(RectTransform), typeof(Image));
                strip.transform.SetParent(c.Root.transform, false);
                var sr = (RectTransform)strip.transform;
                sr.anchorMin = new Vector2(0f, 1f);
                sr.anchorMax = new Vector2(1f, 1f);
                sr.pivot = new Vector2(0.5f, 1f);
                sr.sizeDelta = new Vector2(0f, 12f);
                sr.anchoredPosition = Vector2.zero;
                c.RarityStrip = strip.GetComponent<Image>();

                c.NameText = UIFactory.CreateText(c.Root.transform, "", 28, TextAlignmentOptions.Center);
                var nr = c.NameText.rectTransform;
                nr.anchorMin = new Vector2(0.04f, 0.75f);
                nr.anchorMax = new Vector2(0.96f, 0.92f);
                nr.offsetMin = Vector2.zero; nr.offsetMax = Vector2.zero;

                c.TypeText = UIFactory.CreateText(c.Root.transform, "", 18, TextAlignmentOptions.Center);
                c.TypeText.color = new Color(1f, 1f, 1f, 0.65f);
                var tr = c.TypeText.rectTransform;
                tr.anchorMin = new Vector2(0.04f, 0.62f);
                tr.anchorMax = new Vector2(0.96f, 0.75f);
                tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;

                c.DescText = UIFactory.CreateText(c.Root.transform, "", 18, TextAlignmentOptions.Center);
                var dr = c.DescText.rectTransform;
                dr.anchorMin = new Vector2(0.08f, 0.12f);
                dr.anchorMax = new Vector2(0.92f, 0.60f);
                dr.offsetMin = Vector2.zero; dr.offsetMax = Vector2.zero;
                c.DescText.enableWordWrapping = true;

                return c;
            }

            public void Bind(PickRoller.PickOption? option, System.Action<PickRoller.PickOption?> onClick)
            {
                _option = option;
                _onClick = onClick;

                if (!option.HasValue)
                {
                    Root.SetActive(false);
                    return;
                }
                Root.SetActive(true);

                var opt = option.Value;
                NameText.text = opt.DisplayName;
                TypeText.text = opt.IsHammer ? "HAMMER" : "PASSIVE";
                DescText.text = opt.Description;
                RarityStrip.color = RarityColors.For(opt.Rarity);
                NameText.color = RarityColors.For(opt.Rarity);
            }
        }
    }
}
