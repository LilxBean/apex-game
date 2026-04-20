using System;
using TMPro;
using UnityEngine;

namespace APEX.UI
{
    /// <summary>
    /// Single-page credits overlay. Back routes to onClose.
    /// </summary>
    public class CreditsScreen : MonoBehaviour
    {
        private GameObject _root;
        private Action _onClose;

        public void Init(Transform parent, Action onClose)
        {
            _onClose = onClose;
            BuildUI(parent);
        }

        private void BuildUI(Transform parent)
        {
            _root = new GameObject("CreditsScreen_Root", typeof(RectTransform));
            _root.transform.SetParent(parent, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            UIFactory.CreateFullscreenPanel(_root.transform, new Color(0f, 0f, 0f, 0.85f));

            var title = UIFactory.CreateText(_root.transform, "Credits", 56, TextAlignmentOptions.Center);
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 0.82f);
            titleRt.anchorMax = new Vector2(1f, 0.94f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            var body = UIFactory.CreateText(_root.transform,
                "APEX\nby Brennen Kennedy (LilxBean)\nBuilt with Claude Code.",
                32, TextAlignmentOptions.Center);
            var br = body.rectTransform;
            br.anchorMin = new Vector2(0.1f, 0.30f);
            br.anchorMax = new Vector2(0.9f, 0.78f);
            br.offsetMin = Vector2.zero;
            br.offsetMax = Vector2.zero;

            var btn = UIFactory.CreateButton(_root.transform, "Back");
            var btnRt = (RectTransform)btn.transform;
            btnRt.anchorMin = new Vector2(0.5f, 0.10f);
            btnRt.anchorMax = new Vector2(0.5f, 0.10f);
            btnRt.pivot = new Vector2(0.5f, 0.5f);
            btnRt.sizeDelta = new Vector2(280f, 64f);
            btnRt.anchoredPosition = Vector2.zero;
            btn.onClick.AddListener(OnBackClicked);
        }

        private void OnBackClicked()
        {
            if (_root != null) Destroy(_root);
            _onClose?.Invoke();
        }
    }
}
