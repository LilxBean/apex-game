using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using APEX.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APEX.UI
{
    /// <summary>
    /// Summary of lifetime stats plus a scrollable per-run list, rendered from the
    /// records StatsRecorder persists to stats.json. Built programmatically via UIFactory;
    /// no prefabs. Back button returns to the main menu.
    /// </summary>
    public class StatsScreen : MonoBehaviour
    {
        private static readonly Color VictoryColor = new(1f, 0.847f, 0.420f, 1f);
        private static readonly Color DefeatColor = new(0.816f, 0.251f, 0.251f, 1f);
        private static readonly Color QuitColor = new(0.533f, 0.533f, 0.533f, 1f);
        private static readonly Color RowAltTint = new(1f, 1f, 1f, 0.04f);

        private GameObject _root;
        private Action _onClose;

        public void Init(Transform parent, Action onClose)
        {
            _onClose = onClose;
            BuildUI(parent);
        }

        private void BuildUI(Transform parent)
        {
            _root = new GameObject("StatsScreen_Root", typeof(RectTransform));
            _root.transform.SetParent(parent, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            UIFactory.CreateFullscreenPanel(_root.transform, new Color(0f, 0f, 0f, 0.85f));

            var title = UIFactory.CreateText(_root.transform, "STATS", 56, TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 0.88f);
            titleRt.anchorMax = new Vector2(1f, 0.97f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            IReadOnlyList<RunRecord> records = StatsRecorder.Instance != null
                ? StatsRecorder.Instance.GetAll()
                : Array.Empty<RunRecord>();

            if (records == null || records.Count == 0)
            {
                BuildEmptyState();
            }
            else
            {
                BuildSummary(records);
                BuildDivider();
                BuildRunList(records);
            }

            var btn = UIFactory.CreateButton(_root.transform, "Back");
            var btnRt = (RectTransform)btn.transform;
            btnRt.anchorMin = new Vector2(0.5f, 0f);
            btnRt.anchorMax = new Vector2(0.5f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0f);
            btnRt.sizeDelta = new Vector2(280f, 64f);
            btnRt.anchoredPosition = new Vector2(0f, 24f);
            btn.onClick.AddListener(OnBackClicked);
        }

        private void BuildEmptyState()
        {
            var body = UIFactory.CreateText(_root.transform,
                "No runs recorded yet. Finish a run to start tracking.",
                28, TextAlignmentOptions.Center);
            var br = body.rectTransform;
            br.anchorMin = new Vector2(0.1f, 0.35f);
            br.anchorMax = new Vector2(0.9f, 0.75f);
            br.offsetMin = Vector2.zero;
            br.offsetMax = Vector2.zero;
        }

        private void BuildSummary(IReadOnlyList<RunRecord> records)
        {
            var ci = CultureInfo.InvariantCulture;

            int total = records.Count;
            int victories = 0, defeats = 0, quits = 0;
            float bestTime = 0f, peakDps = 0f, totalPlaytime = 0f;
            int highestLevel = 0, mostKills = 0;

            foreach (var r in records)
            {
                switch (r.endReason)
                {
                    case "Victory": victories++; break;
                    case "Defeat": defeats++; break;
                    default: quits++; break;
                }
                if (r.stats.RunDurationSeconds > bestTime) bestTime = r.stats.RunDurationSeconds;
                if (r.stats.LevelReached > highestLevel) highestLevel = r.stats.LevelReached;
                if (r.stats.Kills > mostKills) mostKills = r.stats.Kills;
                if (r.stats.PeakDPS > peakDps) peakDps = r.stats.PeakDPS;
                totalPlaytime += r.stats.RunDurationSeconds;
            }

            float winPct = total > 0 ? victories * 100f / total : 0f;

            string labels = string.Join("\n",
                "Total Runs",
                "Victories",
                "Defeats",
                "Quits",
                "Best Time",
                "Highest Level",
                "Most Kills",
                "Peak DPS (all)",
                "Total Playtime");
            string values = string.Join("\n",
                total.ToString("N0", ci),
                $"{victories}   ({winPct.ToString("N1", ci)}%)",
                defeats.ToString("N0", ci),
                quits.ToString("N0", ci),
                FormatMMSS(bestTime),
                highestLevel.ToString(ci),
                mostKills.ToString("N0", ci),
                peakDps.ToString("N1", ci),
                FormatHMMSS(totalPlaytime));

            var labelsText = UIFactory.CreateText(_root.transform, labels, 22, TextAlignmentOptions.TopRight);
            labelsText.color = new Color(1f, 1f, 1f, 0.72f);
            var lr = labelsText.rectTransform;
            lr.anchorMin = new Vector2(0.22f, 0.58f);
            lr.anchorMax = new Vector2(0.48f, 0.87f);
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = Vector2.zero;

            var valuesText = UIFactory.CreateText(_root.transform, values, 22, TextAlignmentOptions.TopLeft);
            var vr = valuesText.rectTransform;
            vr.anchorMin = new Vector2(0.50f, 0.58f);
            vr.anchorMax = new Vector2(0.78f, 0.87f);
            vr.offsetMin = Vector2.zero;
            vr.offsetMax = Vector2.zero;
        }

        private void BuildDivider()
        {
            var divider = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            divider.transform.SetParent(_root.transform, false);
            var dr = (RectTransform)divider.transform;
            dr.anchorMin = new Vector2(0.12f, 0.555f);
            dr.anchorMax = new Vector2(0.88f, 0.555f);
            dr.pivot = new Vector2(0.5f, 0.5f);
            dr.sizeDelta = new Vector2(0f, 2f);
            dr.anchoredPosition = Vector2.zero;
            var img = divider.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.25f);
            img.sprite = UIFactory.GetWhiteSprite();
        }

        private void BuildRunList(IReadOnlyList<RunRecord> records)
        {
            var holder = new GameObject("ScrollHolder", typeof(RectTransform));
            holder.transform.SetParent(_root.transform, false);
            var hr = (RectTransform)holder.transform;
            hr.anchorMin = new Vector2(0.08f, 0.14f);
            hr.anchorMax = new Vector2(0.92f, 0.54f);
            hr.offsetMin = Vector2.zero;
            hr.offsetMax = Vector2.zero;

            var (scrollRoot, content) = UIFactory.CreateScrollView(holder.transform, new Vector2(100f, 100f));
            var srRt = (RectTransform)scrollRoot.transform;
            srRt.anchorMin = Vector2.zero;
            srRt.anchorMax = Vector2.one;
            srRt.offsetMin = Vector2.zero;
            srRt.offsetMax = Vector2.zero;

            // Header row (not alternating).
            BuildListRow(content, isHeader: true, tint: false,
                date: "Date", reason: "Result", reasonColor: new Color(1f, 1f, 1f, 0.72f),
                time: "Time", level: "Level", kills: "Kills");

            var sorted = records.OrderByDescending(r => ParseTimestamp(r.timestamp)).ToList();
            int now = DateTime.Now.Year;
            for (int i = 0; i < sorted.Count; i++)
            {
                var r = sorted[i];
                bool tint = i % 2 == 1;
                var local = ParseTimestamp(r.timestamp).ToLocalTime();
                string dateLabel = local.Year == now
                    ? local.ToString("MMM d  HH:mm", CultureInfo.InvariantCulture)
                    : local.ToString("MMM d yyyy  HH:mm", CultureInfo.InvariantCulture);

                BuildListRow(content, isHeader: false, tint: tint,
                    date: dateLabel,
                    reason: r.endReason,
                    reasonColor: ColorForReason(r.endReason),
                    time: FormatMMSS(r.stats.RunDurationSeconds),
                    level: $"L{r.stats.LevelReached}",
                    kills: r.stats.Kills.ToString("N0", CultureInfo.InvariantCulture));
            }
        }

        private static void BuildListRow(
            Transform parent, bool isHeader, bool tint,
            string date, string reason, Color reasonColor,
            string time, string level, string kills)
        {
            var row = new GameObject(isHeader ? "HeaderRow" : "Row", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            var img = row.GetComponent<Image>();
            img.sprite = UIFactory.GetWhiteSprite();
            img.color = tint ? RowAltTint : new Color(0f, 0f, 0f, 0f);

            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = isHeader ? 36f : 32f;
            le.minHeight = le.preferredHeight;

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 2, 2);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            AddCell(row.transform, date, 240f, TextAlignmentOptions.MidlineLeft, isHeader, null);
            AddCell(row.transform, reason, 140f, TextAlignmentOptions.MidlineLeft, isHeader, reasonColor);
            AddCell(row.transform, time, 120f, TextAlignmentOptions.MidlineLeft, isHeader, null);
            AddCell(row.transform, level, 100f, TextAlignmentOptions.MidlineLeft, isHeader, null);
            AddCell(row.transform, kills, 140f, TextAlignmentOptions.MidlineLeft, isHeader, null);
        }

        private static void AddCell(
            Transform parent, string text, float width, TextAlignmentOptions align, bool isHeader, Color? colorOverride)
        {
            float fontSize = isHeader ? 20f : 20f;
            var t = UIFactory.CreateText(parent, text, fontSize, align);
            if (colorOverride.HasValue) t.color = colorOverride.Value;
            else if (isHeader) t.color = new Color(1f, 1f, 1f, 0.60f);

            if (isHeader) t.fontStyle = FontStyles.Bold;

            var le = t.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minWidth = width;
            le.flexibleWidth = 0f;
        }

        private static Color ColorForReason(string reason) => reason switch
        {
            "Victory" => VictoryColor,
            "Defeat" => DefeatColor,
            _ => QuitColor
        };

        private static DateTime ParseTimestamp(string ts)
        {
            if (DateTime.TryParse(ts, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            {
                return dt;
            }
            return DateTime.MinValue;
        }

        private static string FormatMMSS(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int mm = Mathf.FloorToInt(seconds / 60f);
            int ss = Mathf.FloorToInt(seconds % 60f);
            return $"{mm:00}:{ss:00}";
        }

        private static string FormatHMMSS(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int total = Mathf.FloorToInt(seconds);
            int hh = total / 3600;
            int mm = (total % 3600) / 60;
            int ss = total % 60;
            return $"{hh}:{mm:00}:{ss:00}";
        }

        private void OnBackClicked()
        {
            if (_root != null) Destroy(_root);
            _onClose?.Invoke();
        }
    }
}
