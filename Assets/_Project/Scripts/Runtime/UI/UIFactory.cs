using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Minimal helpers for building screen-space UI trees at runtime so scenes don't need
    /// hand-authored prefabs. Tailored to the overlay screens in the Progression folder.
    /// </summary>
    internal static class UIFactory
    {
        private static Sprite _whiteSprite;

        /// <summary>
        /// Returns a runtime-built 4x4 white sprite suitable as a Source Image for
        /// UGUI Image components. Required by Image.Type.Filled — an Image without a
        /// sprite renders as a solid rectangle that ignores fillAmount.
        /// </summary>
        public static Sprite GetWhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "UIFactory_WhiteRuntime",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(pixels);
            tex.Apply(false, true);

            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            _whiteSprite.name = "UIFactory_WhiteRuntime";
            return _whiteSprite;
        }

        public static GameObject CreateOverlayCanvas(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return go;
        }

        public static GameObject CreateFullscreenPanel(Transform parent, Color color)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            return go;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string content, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(Transform parent, string label)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.4f, 1f);

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            var text = CreateText(go.transform, label, 28f, TextAlignmentOptions.Center);
            text.raycastTarget = false;

            return btn;
        }

        /// <summary>
        /// Horizontal slider with a filled track and a draggable handle. No label — callers
        /// add one adjacent if they want. Returned component has Value, onValueChanged.
        /// </summary>
        public static Slider CreateSlider(Transform parent, float min, float max, float initial)
        {
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.14f, 1f);
            bg.sprite = GetWhiteSprite();

            var slider = go.AddComponent<Slider>();

            // Fill Area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRt = (RectTransform)fillArea.transform;
            faRt.anchorMin = new Vector2(0f, 0.25f);
            faRt.anchorMax = new Vector2(1f, 0.75f);
            faRt.offsetMin = new Vector2(10f, 0f);
            faRt.offsetMax = new Vector2(-10f, 0f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg = fill.GetComponent<Image>();
            fillImg.color = new Color(0.2f, 0.6f, 0.4f, 1f);
            fillImg.sprite = GetWhiteSprite();

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var haRt = (RectTransform)handleArea.transform;
            haRt.anchorMin = new Vector2(0f, 0f);
            haRt.anchorMax = new Vector2(1f, 1f);
            haRt.offsetMin = new Vector2(10f, 0f);
            haRt.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var hRt = (RectTransform)handle.transform;
            hRt.sizeDelta = new Vector2(20f, 0f);
            var hImg = handle.GetComponent<Image>();
            hImg.color = Color.white;
            hImg.sprite = GetWhiteSprite();

            slider.targetGraphic = hImg;
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.value = Mathf.Clamp(initial, min, max);

            return slider;
        }

        /// <summary>
        /// Vertical-only scroll view with an auto-sizing content rect. Returns the root
        /// (for placement / sizing) and the content rect (for adding children). Content
        /// is laid out via VerticalLayoutGroup + ContentSizeFitter so callers just append
        /// children at their natural size.
        /// </summary>
        public static (GameObject root, RectTransform content) CreateScrollView(Transform parent, Vector2 size)
        {
            var root = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.sizeDelta = size;

            var bg = root.GetComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.04f);
            bg.sprite = GetWhiteSprite();

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(root.transform, false);
            var viewportRt = (RectTransform)viewportGo.transform;
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            var viewportImg = viewportGo.GetComponent<Image>();
            viewportImg.color = new Color(1f, 1f, 1f, 0.01f);
            viewportImg.sprite = GetWhiteSprite();
            var mask = viewportGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = (RectTransform)contentGo.transform;
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 0f);

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollbar = BuildVerticalScrollbar(root.transform);

            var scroll = root.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.viewport = viewportRt;
            scroll.content = contentRt;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scroll.verticalScrollbarSpacing = 0f;

            return (root, contentRt);
        }

        // Thin vertical scrollbar anchored to the scroll root's right edge. Built to match the
        // runtime white-sprite style used elsewhere in this factory; no prefabs.
        private static Scrollbar BuildVerticalScrollbar(Transform scrollRoot)
        {
            const float width = 10f;

            var barGo = new GameObject("Scrollbar Vertical",
                typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            barGo.transform.SetParent(scrollRoot, false);
            var barRt = (RectTransform)barGo.transform;
            barRt.anchorMin = new Vector2(1f, 0f);
            barRt.anchorMax = new Vector2(1f, 1f);
            barRt.pivot = new Vector2(1f, 0.5f);
            barRt.sizeDelta = new Vector2(width, 0f);
            barRt.anchoredPosition = Vector2.zero;

            var barImg = barGo.GetComponent<Image>();
            barImg.sprite = GetWhiteSprite();
            barImg.color = new Color(1f, 1f, 1f, 0.08f);

            var slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(barGo.transform, false);
            var slideRt = (RectTransform)slidingArea.transform;
            slideRt.anchorMin = Vector2.zero;
            slideRt.anchorMax = Vector2.one;
            slideRt.offsetMin = new Vector2(1f, 1f);
            slideRt.offsetMax = new Vector2(-1f, -1f);

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleGo.transform.SetParent(slidingArea.transform, false);
            var handleRt = (RectTransform)handleGo.transform;
            handleRt.anchorMin = Vector2.zero;
            handleRt.anchorMax = Vector2.one;
            handleRt.offsetMin = Vector2.zero;
            handleRt.offsetMax = Vector2.zero;
            var handleImg = handleGo.GetComponent<Image>();
            handleImg.sprite = GetWhiteSprite();
            handleImg.color = new Color(1f, 1f, 1f, 0.55f);

            var scrollbar = barGo.GetComponent<Scrollbar>();
            scrollbar.targetGraphic = handleImg;
            scrollbar.handleRect = handleRt;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            return scrollbar;
        }

        /// <summary>
        /// Toggle with a square checkbox on the left and a label on the right. The label
        /// is raycast-disabled so only the checkbox hitbox consumes clicks.
        /// </summary>
        public static Toggle CreateToggle(Transform parent, string label, bool initial)
        {
            var go = new GameObject("Toggle", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var toggle = go.AddComponent<Toggle>();
            toggle.isOn = initial;

            // Checkbox background (acts as targetGraphic so the full row is clickable
            // via the graphic; we set it transparent so only the visible box shows).
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(0f, 0.5f);
            bgRt.pivot = new Vector2(0f, 0.5f);
            bgRt.sizeDelta = new Vector2(32f, 32f);
            bgRt.anchoredPosition = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0.12f, 0.12f, 0.14f, 1f);
            bgImg.sprite = GetWhiteSprite();

            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            var cRt = (RectTransform)check.transform;
            cRt.anchorMin = new Vector2(0.15f, 0.15f);
            cRt.anchorMax = new Vector2(0.85f, 0.85f);
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;
            var cImg = check.GetComponent<Image>();
            cImg.color = new Color(0.2f, 0.85f, 0.5f, 1f);
            cImg.sprite = GetWhiteSprite();

            toggle.targetGraphic = bgImg;
            toggle.graphic = cImg;

            var text = CreateText(go.transform, label, 22f, TextAlignmentOptions.MidlineLeft);
            var tRt = text.rectTransform;
            tRt.anchorMin = new Vector2(0f, 0f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.offsetMin = new Vector2(44f, 0f);
            tRt.offsetMax = new Vector2(0f, 0f);

            return toggle;
        }
    }
}
