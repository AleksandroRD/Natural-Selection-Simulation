using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace GUIUtils
{
    public class LiveLineChart
    {
        private Vector2 cursorPosition = default;
        private readonly ChartConfig config;

        private SortedDictionary<float,float> data;

        private Dictionary<int, (float alpha, string text)> timeLabelData = new();
        private Dictionary<int, (float alpha, string text)> valueLabeldata = new();

        private float timeInterval;
        private float valueInterval;
        /// <summary>
        /// Maximum number of labels per axis. Just for safety.
        /// </summary>
        private const int MAX_LABELS = 30;
        /// <summary>
        /// Magic number for converting float to ints for use as keys.
        /// </summary>
        private const int KEY_SCALE = 1000;

        public LiveLineChart(ChartConfig config)
        {
            this.config = config;

            config.ChartCanvas.generateVisualContent += UpdateChart;

            config.ChartCanvas.RegisterCallback<PointerMoveEvent>((evt) =>
            {
                
                cursorPosition = evt.localPosition;
                //populationToolTipLabel.text = config.ChartCanvas.tooltip;
                config.ChartCanvas.MarkDirtyRepaint(); 
            });

            config.ChartCanvas.RegisterCallback<PointerOverEvent>((ctx) =>
            {
                UnityEngine.Cursor.SetCursor(config.cursorTexture, new Vector2(config.cursorTexture.width / 2, config.cursorTexture.height / 2), CursorMode.Auto);
            });

            config.ChartCanvas.RegisterCallback<PointerOutEvent>((ctx) =>
            {
                //populationToolTipLabel.text = "";
                cursorPosition = Vector3.zero;
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                config.ChartCanvas.MarkDirtyRepaint(); 
            });

        }

        public void UpdateData(SortedDictionary<float,float> data)
        {
            this.data = data;
            config.ChartCanvas.MarkDirtyRepaint();
        }

        public void UpdateData(float key, float value)
        {
            data.Add(key, value);
            config.ChartCanvas.MarkDirtyRepaint();
        }

        private void UpdateChart(MeshGenerationContext ctx)
        {
            if(data == null || data.Count < 2) { return; }
            DrawLineChart(ctx);
            
            float minTime = data.Keys.First();
            float maxTime = data.Keys.Last();
            float timeSpan = maxTime - minTime;
            timeInterval = TimeInterval(timeSpan);
            config.ChartCanvas.schedule.Execute(() => BuildAxisLabels(config.TimeAxisCanvas, timeLabelData, config.TimeAxisCanvas.resolvedStyle.width, config.minTimeLabelGaps, timeInterval, minTime, maxTime));
            
            float minValue = data.Values.Min();
            float maxValue = data.Values.Max();
            float valueRange = maxValue - minValue;
            float pxPerUnit = config.ValueAxisCanvas.resolvedStyle.height / Mathf.Max(valueRange, 1);

            valueInterval = ValueInterval(valueRange, pxPerUnit, config.minValueLabelGaps, valueInterval);
            config.ChartCanvas.schedule.Execute(() => BuildAxisLabels(config.ValueAxisCanvas, valueLabeldata, config.ValueAxisCanvas.resolvedStyle.height, config.minValueLabelGaps, valueInterval, minValue, maxValue));
        }

        private void DrawLineChart(MeshGenerationContext ctx) 
        {
            var painter = ctx.painter2D;

            float minTime = data.Keys.First();
            float maxTime = data.Keys.Last();

            float minValue = data.Min(x => x.Value);
            float maxValue = data.Max(x => x.Value);

            float width = config.ChartCanvas.resolvedStyle.width;
            float height = config.ChartCanvas.resolvedStyle.height;

            //draw line for current value
            painter.strokeColor = config.lineColor;
            painter.lineWidth   = 1f;
            painter.SetDashPattern(8f,5f);
            painter.BeginPath();
            painter.MoveTo(new Vector2(0, height - Mathf.InverseLerp(minValue, maxValue, data.Last().Value) * height));
            painter.LineTo(new Vector2(width, height - Mathf.InverseLerp(minValue, maxValue, data.Last().Value) * height));
            painter.Stroke();
            painter.SetDashPattern(null);
            //draw end

            //draw circle on current value
            painter.strokeColor = Color.white;
            painter.fillColor = config.lineColor;
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.Arc(new Vector2(width, height - Mathf.InverseLerp(minValue, maxValue, data.Last().Value) * height), 6f, 0f, 360f);
            painter.Fill();
            painter.Stroke();
            //end draw

            int counter = 0;
            painter.strokeColor = config.lineColor;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0,height - Mathf.InverseLerp(minValue,maxValue,data.First().Value) * height));
            foreach(var pair in data)
            {
                //this way of calculating x makes point evenly spread throughout the graph, which makes it look better
                float x = (data.Count() > 1) ? counter * width / (data.Count() - 1): 0;
                float y = height - Mathf.InverseLerp(minValue, maxValue,pair.Value) * height;

                painter.LineTo(new Vector2(x, y));
                counter++;
            }
            painter.Stroke();

            if (cursorPosition != Vector2.zero)
            {
                float step = width / (data.Count() - 1);

                // Snap cursor X to nearest discrete column index
                int snappedIndex = Mathf.RoundToInt(cursorPosition.x / step);
                snappedIndex = Mathf.Clamp(snappedIndex, 0, data.Count() - 1);
                float snappedX = snappedIndex * step;

                var keyValuePair = data.ElementAt(snappedIndex); 
                float snappedY = height - Mathf.InverseLerp(minValue, maxValue, keyValuePair.Value) * height;

                config.ChartCanvas.tooltip = $"{config.FormatTime(keyValuePair.Key)} - {config.FormatValue(keyValuePair.Value)}";
                // Vertical cursor line
                painter.strokeColor = Color.lightGray;
                painter.lineWidth = 1f;
                painter.BeginPath();
                painter.MoveTo(new Vector2(snappedX, height));
                painter.LineTo(new Vector2(snappedX, snappedY));
                painter.Stroke();

                // Circle fill (line color)
                painter.fillColor = config.lineColor;
                painter.BeginPath();
                painter.Arc(new Vector2(snappedX, snappedY), 4f, 0f, 360f);
                painter.Fill();
            }
        }

        void BuildAxisLabels(VisualElement container, Dictionary<int, (float alpha, string text)> LabelData, float axisSize, float minGap, float interval, float minValue, float maxValue)
        {
            var targets = new HashSet<int>();
            float first = Mathf.Ceil((minValue - interval) / interval) * interval;
            for (float t = first; t <= maxValue + interval && targets.Count < MAX_LABELS; t += interval)
            {
                targets.Add(Mathf.RoundToInt(ValueToKey(t)));
            }

            // 1. Sync label-state dict with this frame's targets
            foreach (int key in targets)
            {
                string text = KeyToValue(key).ToString();
                if (!LabelData.TryGetValue(key, out var state))
                    LabelData[key] = (0f, text);
                else
                {
                    state.text = text;
                    LabelData[key] = state;
                }
            }

            // 2. Fade alphas toward target; prune fully-faded labels
            foreach (int key in LabelData.Keys.ToList())
            {
                var state  = LabelData[key];
                float norm = Normalize(KeyToValue(key), minValue, maxValue);
                float target = targets.Contains(key) ? EdgeAlpha(norm, axisSize) : 0f;

                float next = Mathf.Lerp(state.alpha, target, config.LabelFade);
                if (Mathf.Abs(next - target) < 0.02f) { next = target; }

                if (next < 0.01f && target == 0f) { LabelData.Remove(key); continue; }

                state.alpha = next;
                LabelData[key] = state;
            }

            // 3. Collect labels that are visible and within range
            var visible = new List<(float pos, float alpha, string text)>();
            foreach (var (key, state) in LabelData)
            {
                if (state.alpha < 0.02f) continue;

                float pos = Normalize(KeyToValue(key), minValue, maxValue);
                if (pos < -0.05f || pos > 1.05f) continue;

                visible.Add((pos, state.alpha, state.text));
            }

            // 4. Sort along the axis (ascending position)
            visible.Sort((a, b) => a.pos.CompareTo(b.pos));

            // 5. Greedy overlap resolution — keep higher-alpha label on collision
            var drawn = new List<(float pos, float alpha, string text)>();
            foreach (var label in visible)
            {
                if (drawn.Count > 0)
                {
                    var prev   = drawn[^1];
                    float gap  = Mathf.Abs(label.pos - prev.pos) * axisSize;
                    if (gap < minGap)
                    {
                        if (label.alpha > prev.alpha) drawn[^1] = label;
                        continue;
                    }
                }
                drawn.Add(label);
            }

            // 6. Write UI elements
            container.Clear();
            foreach (var (pos, alpha, text) in drawn)
            {
                DrawLabel(container, pos, alpha, text);
            }
        }

        private void DrawLabel(VisualElement container, float pos, float alpha, string text)
        {
            var lbl = new Label(text);
            lbl.AddToClassList("axis-text");
            lbl.style.opacity  = alpha;
            lbl.style.position = Position.Absolute;

            //figure out if this container is vertical or horizontal
            if (container.resolvedStyle.height > container.resolvedStyle.width)
            {
                lbl.style.top       = (1f - pos) * container.resolvedStyle.height;
                lbl.style.translate = new StyleTranslate(new Translate(0, Length.Percent(-50)));
            }
            else
            {
                lbl.style.left      = pos * container.resolvedStyle.width;
                lbl.style.translate = new StyleTranslate(new Translate(Length.Percent(-50), 0));
            }

            container.Add(lbl);
        }

        float ValueInterval(float valRange, float pxPerUnit, float minGap, float previousInterval)
        {
            if (previousInterval > 0)
            {
                float px = previousInterval * pxPerUnit;
                if (px >= minGap * 0.5 && px <= minGap * 4)
                    return previousInterval;
            }

            float[][] divisorSets =
            {
                new float[] { 2, 2.5f, 2 },
                new float[] { 2, 2, 2.5f },
                new float[] { 2.5f, 2, 2 }
            };

            float best = float.PositiveInfinity;

            foreach (var divs in divisorSets)
            {
                float span = Mathf.Pow(10, Mathf.Ceil(Mathf.Log10(valRange)));
                int i = 0;

                while ((span / divs[i % 3]) * pxPerUnit >= minGap)
                {
                    span /= divs[i % 3];
                    i++;
                }

                if (span < best)
                    best = span;
            }

            return float.IsPositiveInfinity(best)
                ? valRange / 5
                : best;
        }

        float TimeInterval(float spanSecs)
        {
            float[] steps = { 1, 2, 5, 10, 15, 30, 60, 120, 300, 600, 1800, 3600, 7200, 21600, 86400 };
            float target  = spanSecs / 6f;
            foreach (float state in steps)
            {
                if (state >= target) return state;
            }
            return steps[^1];
        }

        float EdgeAlpha(float norm, float axisSize)
        {
            float fromEdge = Mathf.Min(norm, 1f - norm) * axisSize;
            return Mathf.Clamp01(fromEdge / config.FadeZone);
        }

        float Normalize(float value, float minValue, float maxValue)
        {
            return (value - minValue) / (maxValue - minValue);
        }

        float KeyToValue(int value)
        {
            return value / KEY_SCALE;
        }

        int ValueToKey(float value)
        {
            return Mathf.RoundToInt(value * KEY_SCALE);
        }

        public void Enable()
        {
            config.ChartCanvas.generateVisualContent -= UpdateChart;
            config.ChartCanvas.generateVisualContent += UpdateChart;
        }

        /// <summary>
        /// Prevents chart from updating.
        /// </summary>
        public void Disable()
        {
            config.ChartCanvas.generateVisualContent -= UpdateChart;
        }
    }
    public record ChartConfig
    {
        /// <summary>
        /// <see cref=" VisualElement"/> where the chart would be drawn.
        /// </summary>
        public VisualElement ChartCanvas;
        /// <summary>
        /// <see cref=" VisualElement"/> where time intervals would be displayed.
        /// </summary>
        public VisualElement TimeAxisCanvas;
        /// <summary>
        /// <see cref=" VisualElement"/> where value intervals would be displayed.
        /// </summary>
        public VisualElement ValueAxisCanvas;
        /// <summary>
        /// Color with which main chart line is drawn.
        /// </summary>
        public Color lineColor;
        /// <summary>
        /// Function with which time value converted to a string e.g. 124.5679 -> 124.7s
        /// </summary>
        public Func<float,string> FormatTime;
        /// <summary>
        /// Function with which time value converted to a string e.g. 124.5679 -> 124.7
        /// </summary>
        public Func<float,string> FormatValue;

        /// <summary>
        /// Distance to the edge of the AxisCanvas where labels start to fade.
        /// </summary>
        public float FadeZone;
        public float LabelFade;

        public float minTimeLabelGaps;
        public float minValueLabelGaps;

        public Texture2D cursorTexture;
    }
}

