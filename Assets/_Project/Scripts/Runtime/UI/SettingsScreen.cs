using System;
using APEX.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Overlay settings editor. Bound to SettingsService.Instance.Store. Re-usable from a
    /// future pause menu: pass an onClose callback; Back triggers Save + callback.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
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
            _root = new GameObject("SettingsScreen_Root", typeof(RectTransform));
            _root.transform.SetParent(parent, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            UIFactory.CreateFullscreenPanel(_root.transform, new Color(0f, 0f, 0f, 0.85f));

            var title = UIFactory.CreateText(_root.transform, "Settings", 56, TextAlignmentOptions.Center);
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 0.82f);
            titleRt.anchorMax = new Vector2(1f, 0.94f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            var service = SettingsService.Instance;
            if (service == null)
            {
                Debug.LogWarning("[APEX] SettingsScreen opened with no SettingsService; controls will no-op.");
                return;
            }

            var store = service.Store;

            // Column container.
            var column = new GameObject("Controls", typeof(RectTransform));
            column.transform.SetParent(_root.transform, false);
            var colRt = (RectTransform)column.transform;
            colRt.anchorMin = new Vector2(0.5f, 0.30f);
            colRt.anchorMax = new Vector2(0.5f, 0.80f);
            colRt.pivot = new Vector2(0.5f, 0.5f);
            colRt.sizeDelta = new Vector2(520f, 0f);
            colRt.anchoredPosition = Vector2.zero;
            var vlayout = column.AddComponent<VerticalLayoutGroup>();
            vlayout.spacing = 18f;
            vlayout.childAlignment = TextAnchor.UpperCenter;
            vlayout.childControlWidth = true;
            vlayout.childControlHeight = true;
            vlayout.childForceExpandWidth = true;
            vlayout.childForceExpandHeight = false;

            BuildVolumeRow(column.transform, store, service);
            BuildToggleRow(column.transform, "Fullscreen", store.fullscreen, isOn =>
            {
                store.fullscreen = isOn;
                service.Apply();
            });
            BuildToggleRow(column.transform, "VSync", store.vsync, isOn =>
            {
                store.vsync = isOn;
                service.Apply();
            });

            BuildBackButton();
        }

        private void BuildVolumeRow(Transform parent, SettingsStore store, SettingsService service)
        {
            var row = new GameObject("VolumeRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 44f;
            le.minHeight = 44f;

            var label = UIFactory.CreateText(row.transform, "Master Volume", 22, TextAlignmentOptions.MidlineLeft);
            var lr = label.rectTransform;
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(0.4f, 1f);
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = Vector2.zero;

            var slider = UIFactory.CreateSlider(row.transform, 0f, 1f, store.masterVolume);
            var sr = (RectTransform)slider.transform;
            sr.anchorMin = new Vector2(0.4f, 0.25f);
            sr.anchorMax = new Vector2(1f, 0.75f);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = Vector2.zero;
            slider.onValueChanged.AddListener(v =>
            {
                store.masterVolume = v;
                // No audio side-effect this session.
            });
        }

        private void BuildToggleRow(Transform parent, string label, bool initial, Action<bool> onChanged)
        {
            var row = new GameObject($"{label}Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 40f;
            le.minHeight = 40f;

            var toggle = UIFactory.CreateToggle(row.transform, label, initial);
            var tr = (RectTransform)toggle.transform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            toggle.onValueChanged.AddListener(v => onChanged(v));
        }

        private void BuildBackButton()
        {
            var btn = UIFactory.CreateButton(_root.transform, "Back");
            var br = (RectTransform)btn.transform;
            br.anchorMin = new Vector2(0.5f, 0.10f);
            br.anchorMax = new Vector2(0.5f, 0.10f);
            br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(280f, 64f);
            br.anchoredPosition = Vector2.zero;
            btn.onClick.AddListener(OnBackClicked);
        }

        private void OnBackClicked()
        {
            SettingsService.Instance?.Save();
            if (_root != null) Destroy(_root);
            _onClose?.Invoke();
        }
    }
}
