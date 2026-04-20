using APEX.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Root menu screen in MainMenu.unity. Built programmatically — no prefab dependency.
    /// Sub-screens (Settings, Stats, Credits) are built on demand as siblings of the main
    /// button stack and hide it while open.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        private GameObject _root;
        private GameObject _buttonStack;

        private SettingsScreen _settingsScreen;
        private StatsScreen _statsScreen;
        private CreditsScreen _creditsScreen;

        public void Build(Transform canvasParent)
        {
            _root = new GameObject("MainMenuScreen_Root", typeof(RectTransform));
            _root.transform.SetParent(canvasParent, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = UIFactory.CreateFullscreenPanel(_root.transform, new Color(0.05f, 0.05f, 0.07f, 1f));
            bg.name = "Background";

            var title = UIFactory.CreateText(_root.transform, "APEX", 128, TextAlignmentOptions.Center);
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 0.78f);
            titleRt.anchorMax = new Vector2(1f, 0.94f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            BuildButtonStack();
        }

        private const int ButtonCount = 7;
        private const float ButtonHeight = 56f;
        private const float ButtonSpacing = 14f;

        private void BuildButtonStack()
        {
            _buttonStack = new GameObject("ButtonStack", typeof(RectTransform));
            _buttonStack.transform.SetParent(_root.transform, false);
            var stackRt = (RectTransform)_buttonStack.transform;
            // Fixed-size centered stack: width 420, height = N*h + (N-1)*spacing.
            // Offset slightly below center to clear the title.
            float stackHeight = ButtonCount * ButtonHeight + (ButtonCount - 1) * ButtonSpacing;
            stackRt.anchorMin = new Vector2(0.5f, 0.5f);
            stackRt.anchorMax = new Vector2(0.5f, 0.5f);
            stackRt.pivot = new Vector2(0.5f, 0.5f);
            stackRt.sizeDelta = new Vector2(420f, stackHeight);
            stackRt.anchoredPosition = new Vector2(0f, -60f);

            var layout = _buttonStack.AddComponent<VerticalLayoutGroup>();
            layout.spacing = ButtonSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddButton("Main Game", () => SceneLoader.Run(SceneLoader.StartRun(RunRequest.Default(Mode.Main))), true);
            AddButton("Challenge Mode — Coming Soon", null, false);
            AddButton("Endless — Coming Soon", null, false);
            AddButton("Settings", OpenSettings, true);
            AddButton("Stats", OpenStats, true);
            AddButton("Credits", OpenCredits, true);
            AddButton("Exit", OnExitClicked, true);
        }

        private void AddButton(string label, System.Action onClick, bool enabled)
        {
            var btn = UIFactory.CreateButton(_buttonStack.transform, label);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 56f;
            le.minHeight = 56f;

            if (!enabled)
            {
                btn.interactable = false;
                var img = btn.GetComponent<Image>();
                img.color = new Color(0.25f, 0.25f, 0.28f, 1f);
                var label_ = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label_ != null) label_.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick());
            }
        }

        private void OpenSettings()
        {
            SetStackVisible(false);
            _settingsScreen = gameObject.AddComponent<SettingsScreen>();
            _settingsScreen.Init(_root.transform, OnSubScreenClosed);
        }

        private void OpenStats()
        {
            SetStackVisible(false);
            _statsScreen = gameObject.AddComponent<StatsScreen>();
            _statsScreen.Init(_root.transform, OnSubScreenClosed);
        }

        private void OpenCredits()
        {
            SetStackVisible(false);
            _creditsScreen = gameObject.AddComponent<CreditsScreen>();
            _creditsScreen.Init(_root.transform, OnSubScreenClosed);
        }

        private void OnSubScreenClosed()
        {
            if (_settingsScreen != null) { Destroy(_settingsScreen); _settingsScreen = null; }
            if (_statsScreen != null)    { Destroy(_statsScreen);    _statsScreen = null; }
            if (_creditsScreen != null)  { Destroy(_creditsScreen);  _creditsScreen = null; }

            SetStackVisible(true);
        }

        private void SetStackVisible(bool visible)
        {
            if (_buttonStack != null) _buttonStack.SetActive(visible);
        }

        private void OnExitClicked()
        {
#if UNITY_EDITOR
            Debug.Log("[APEX] Exit (editor no-op)");
#else
            Application.Quit();
#endif
        }
    }
}
