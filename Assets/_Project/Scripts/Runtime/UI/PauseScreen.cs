using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Translucent modal shown while the run is paused. Built entirely via UIFactory so
    /// the scene doesn't need a hand-authored prefab. Clicking any button fires the matching
    /// callback; the owner (PauseController) decides what to do next.
    /// </summary>
    public class PauseScreen : MonoBehaviour
    {
        /// <summary>Button callbacks wired by the PauseController that owns this screen.</summary>
        public struct Callbacks
        {
            public Action OnResume;
            public Action OnSettings;
            public Action OnRestart;
            public Action OnEndRun;
            public Action OnQuitToMenu;
        }

        private GameObject _root;

        /// <summary>Sibling canvas root — safe to SetActive from the controller to hide without destroy.</summary>
        public GameObject Root => _root;

        public void Init(Transform parent, Callbacks callbacks)
        {
            BuildUI(parent, callbacks);
        }

        public void Show()
        {
            if (_root != null)
            {
                _root.SetActive(true);
                _root.transform.SetAsLastSibling();
            }
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        private void BuildUI(Transform parent, Callbacks callbacks)
        {
            _root = new GameObject("PauseScreen_Root", typeof(RectTransform));
            _root.transform.SetParent(parent, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            UIFactory.CreateFullscreenPanel(_root.transform, new Color(0f, 0f, 0f, 0.60f));

            var title = UIFactory.CreateText(_root.transform, "PAUSED", 88f, TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 0.76f);
            titleRt.anchorMax = new Vector2(1f, 0.92f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            var column = new GameObject("Buttons", typeof(RectTransform));
            column.transform.SetParent(_root.transform, false);
            var colRt = (RectTransform)column.transform;
            colRt.anchorMin = new Vector2(0.5f, 0.14f);
            colRt.anchorMax = new Vector2(0.5f, 0.72f);
            colRt.pivot = new Vector2(0.5f, 0.5f);
            colRt.sizeDelta = new Vector2(360f, 0f);
            colRt.anchoredPosition = Vector2.zero;

            var layout = column.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddButton(column.transform, "Resume", callbacks.OnResume);
            AddButton(column.transform, "Settings", callbacks.OnSettings);
            AddButton(column.transform, "Restart", callbacks.OnRestart);
            AddButton(column.transform, "End Run", callbacks.OnEndRun);
            AddButton(column.transform, "Quit to Main Menu", callbacks.OnQuitToMenu);
        }

        private static void AddButton(Transform parent, string label, Action callback)
        {
            var btn = UIFactory.CreateButton(parent, label);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.minHeight = 64f;
            if (callback != null) btn.onClick.AddListener(() => callback());
        }
    }
}
