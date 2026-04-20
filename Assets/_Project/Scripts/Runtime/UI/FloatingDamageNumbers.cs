using System.Collections.Generic;
using APEX.Combat;
using APEX.Core.Events;
using APEX.Player;
using TMPro;
using UnityEngine;

namespace APEX.UI
{
    /// <summary>
    /// Spawns pooled floating damage-number labels on EventBus.OnDamaged.
    /// Red when the player is the victim, white for enemies. Runs in screen space
    /// above the HUD; world→screen conversion each frame so numbers track the
    /// hit location as the camera pans.
    /// </summary>
    public class FloatingDamageNumbers : MonoBehaviour
    {
        private const float Lifetime = 0.85f;
        private const float RiseWorldUnits = 1.2f;
        private const int InitialPoolSize = 16;
        private const float FontSize = 28f;

        private static readonly Color PlayerColor = new(0.95f, 0.25f, 0.25f, 1f);
        private static readonly Color EnemyColor = Color.white;

        private RectTransform _canvasRect;
        private Camera _camera;
        private readonly Queue<TextMeshProUGUI> _pool = new();
        private readonly List<Active> _active = new();

        private struct Active
        {
            public TextMeshProUGUI Label;
            public Vector3 WorldOrigin;
            public float Elapsed;
        }

        private void Awake()
        {
            var canvas = UIFactory.CreateOverlayCanvas(transform, "FloatingDamage_Canvas", sortingOrder: 20);
            _canvasRect = (RectTransform)canvas.transform;

            for (int i = 0; i < InitialPoolSize; i++)
            {
                _pool.Enqueue(CreateLabel());
            }
        }

        private void OnEnable() { EventBus.OnDamaged += OnDamaged; }
        private void OnDisable() { EventBus.OnDamaged -= OnDamaged; }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var a = _active[i];
                a.Elapsed += Time.unscaledDeltaTime;
                if (a.Elapsed >= Lifetime)
                {
                    Release(a.Label);
                    _active.RemoveAt(i);
                    continue;
                }

                float t = a.Elapsed / Lifetime;
                var world = a.WorldOrigin + Vector3.up * (RiseWorldUnits * t);
                var screen = _camera.WorldToScreenPoint(world);
                a.Label.rectTransform.position = screen;

                var c = a.Label.color;
                c.a = 1f - t;
                a.Label.color = c;

                _active[i] = a;
            }
        }

        private void OnDamaged(IDamageable target, Damage damage)
        {
            if (damage.Amount <= 0f) return;

            var mb = target as MonoBehaviour;
            if (mb == null) return;

            bool isPlayerVictim = mb.GetComponent<PlayerController>() != null;
            var color = isPlayerVictim ? PlayerColor : EnemyColor;

            var label = _pool.Count > 0 ? _pool.Dequeue() : CreateLabel();
            label.gameObject.SetActive(true);
            label.text = Mathf.CeilToInt(damage.Amount).ToString();
            label.color = color;

            _active.Add(new Active
            {
                Label = label,
                WorldOrigin = mb.transform.position,
                Elapsed = 0f
            });
        }

        private void Release(TextMeshProUGUI label)
        {
            label.gameObject.SetActive(false);
            _pool.Enqueue(label);
        }

        private TextMeshProUGUI CreateLabel()
        {
            var label = UIFactory.CreateText(_canvasRect, "0", FontSize, TextAlignmentOptions.Center);
            label.fontStyle = FontStyles.Bold;
            var rt = label.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(120f, 40f);
            label.gameObject.SetActive(false);
            return label;
        }
    }
}
