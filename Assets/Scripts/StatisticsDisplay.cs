using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum StatisticsDisplayMode
{
    Population,
    Gene
}

[RequireComponent(typeof(UIDocument))]
public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private Color lineColor   = new Color(0.27f, 0.71f, 1f);
    [SerializeField] private float lineWidth   = 2.5f;

    [SerializeField] private Texture2D precisionCursor;
    string chosenGene;

    string chosenCreature = "Rabbit";
    bool showGeneHistory = false;
    int NumberOfDataToShow = 100;

    private Dictionary<int, (float alpha, string text)> populationTimeLabelData = new();
    private Dictionary<int, (float alpha, string text)> populationValueLabeldata = new();

    private Dictionary<int, (float alpha, string text)> geneTimeLabelData = new();
    private Dictionary<int, (float alpha, string text)> geneValueLabeldata = new();

    private const float LABEL_FADE = 0.08f;
    private const int   MAX_LABELS = 30;    // safety cap

    private float valueInterval;
    private Vector2 cursorLocalPosition;

    EnumField      modeDropdown;

    VisualElement  geneHistogramRoot;
    DropdownField  geneDropdown;
    Label          geneOverallCountLabel;
    Label          geneTooltipLabel;
    Label          geneAverageLabel;
    Toggle         geneHistoryToggle;
    VisualElement  geneDistributionCanvas;
    VisualElement  geneHistoryCanvas;
    VisualElement  geneDistributionRoot;
    VisualElement  geneHistoryRoot;
    Label          geneHistoryTooltipLabel;
    VisualElement  secondGeneTop;
    Label          populationToolTipLabel;
    VisualElement  populationChartCanvas;
    VisualElement  populationChartRoot;

    VisualElement  populationXAxis;
    VisualElement  populationYAxis;

    VisualElement  geneXAxis;
    VisualElement  geneYAxis;
    Button         button50;
    Button         button100;
    Button         button250;
    Button         button500;
    Button         buttonAll;

    Button         geneButton50;
    Button         geneButton100;
    Button         geneButton250;
    Button         geneButton500;
    Button         geneButtonAll;

#region Setup
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        modeDropdown = root.Q<EnumField>("mode-dropdown");

        geneHistogramRoot = root.Q<VisualElement>("gene-chart");
        geneDropdown = root.Q<DropdownField>("gene-dropdown");
        geneOverallCountLabel = root.Q<Label>("gene-overall-count-label");
        geneTooltipLabel = root.Q<Label>("gene-chart-tootip-label");
        geneAverageLabel = root.Q<Label>("gene-average-label");
        geneHistoryToggle = root.Q<Toggle>("gene-history-toggle");
        geneDistributionCanvas    = root.Q<VisualElement>("gene-distribution-canvas");
        geneHistoryCanvas = root.Q<VisualElement>("gene-history-chart-canvas");
        geneHistoryRoot = root.Q<VisualElement>("gene-history-chart");
        geneDistributionRoot = root.Q<VisualElement>("gene-distribution-chart");
        geneXAxis = root.Q<VisualElement>("gene-history-axis-x");
        geneYAxis = root.Q<VisualElement>("gene-history-axis-y");
        geneHistoryTooltipLabel = root.Q<Label>("gene-chart-history-tootip-label");
        secondGeneTop = root.Q<VisualElement>("second-gene-top");

        populationChartRoot = root.Q<VisualElement>("population-chart");
        populationChartCanvas = root.Q<VisualElement>("population-chart-canvas");
        populationXAxis = root.Q<VisualElement>("population-chart-axis-x");
        populationYAxis = root.Q<VisualElement>("population-chart-axis-y");
        populationToolTipLabel = root.Q<Label>("population-chart-tooltip-label");

        button50 = root.Q<Button>("50dp-button");
        button100 = root.Q<Button>("100dp-button");
        button250 = root.Q<Button>("250dp-button");
        button500 = root.Q<Button>("500dp-button");
        buttonAll = root.Q<Button>("alldp-button");

        geneButton50 = root.Q<Button>("g-50dp-button");
        geneButton100 = root.Q<Button>("g-100dp-button");
        geneButton250 = root.Q<Button>("g-250dp-button");
        geneButton500 = root.Q<Button>("g-500dp-button");
        geneButtonAll = root.Q<Button>("g-alldp-button");

        geneButton50.clicked += () => {NumberOfDataToShow = 50; geneHistoryCanvas.MarkDirtyRepaint();};
        geneButton100.clicked += () => {NumberOfDataToShow = 100; geneHistoryCanvas.MarkDirtyRepaint();};
        geneButton250.clicked += () => {NumberOfDataToShow = 250; geneHistoryCanvas.MarkDirtyRepaint();};
        geneButton500.clicked += () => {NumberOfDataToShow = 500; geneHistoryCanvas.MarkDirtyRepaint();};
        geneButtonAll.clicked += () => {NumberOfDataToShow = -1; geneHistoryCanvas.MarkDirtyRepaint();};

        button50.clicked += () => {NumberOfDataToShow = 50; populationChartCanvas.MarkDirtyRepaint();};
        button100.clicked += () => {NumberOfDataToShow = 100; populationChartCanvas.MarkDirtyRepaint();};
        button250.clicked += () => {NumberOfDataToShow = 250; populationChartCanvas.MarkDirtyRepaint();};
        button500.clicked += () => {NumberOfDataToShow = 500; populationChartCanvas.MarkDirtyRepaint();};
        buttonAll.clicked += () => {NumberOfDataToShow = -1; populationChartCanvas.MarkDirtyRepaint();};

        geneDropdown.RegisterValueChangedCallback(OnGeneDropdownChanged);
        geneHistoryToggle.RegisterValueChangedCallback(ToggleHistory);
        modeDropdown.RegisterValueChangedCallback(ChangeMode);

        populationChartCanvas.generateVisualContent += DrawPopulationChart;
        geneHistoryCanvas.generateVisualContent += DrawGeneHistoryChart;
        Statistics.OnGeneStatisticsUpdated += UpdateGeneStatistics;
        Statistics.OnPopulationUpdated    += UpdateOverallGeneCount;
        Statistics.OnPopulationUpdated    += populationChartCanvas.MarkDirtyRepaint;

        populationChartCanvas.RegisterCallback<PointerMoveEvent>((evt) =>
        {
            cursorLocalPosition = evt.localPosition;
            populationToolTipLabel.text = populationChartCanvas.tooltip;
            populationChartCanvas.MarkDirtyRepaint(); 
        });

        populationChartCanvas.RegisterCallback<PointerOverEvent>((ctx) =>
        {
            UnityEngine.Cursor.SetCursor(precisionCursor, new Vector2(precisionCursor.width/2,precisionCursor.height/2),CursorMode.Auto);
        });

        populationChartCanvas.RegisterCallback<PointerOutEvent>((ctx) =>
        {
            populationToolTipLabel.text = "";
            cursorLocalPosition = Vector3.zero;
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            populationChartCanvas.MarkDirtyRepaint(); 
        });

        geneHistoryCanvas.RegisterCallback<PointerMoveEvent>((evt) =>
        {
            cursorLocalPosition = evt.localPosition;
            geneHistoryTooltipLabel.text = geneHistoryCanvas.tooltip;
            geneHistoryCanvas.MarkDirtyRepaint(); 
        });

        geneHistoryCanvas.RegisterCallback<PointerOverEvent>((ctx) =>
        {
            UnityEngine.Cursor.SetCursor(precisionCursor, new Vector2(precisionCursor.width/2,precisionCursor.height/2),CursorMode.Auto);
        });

        geneHistoryCanvas.RegisterCallback<PointerOutEvent>((ctx) =>
        {
            geneHistoryTooltipLabel.text = "";
            cursorLocalPosition = Vector3.zero;
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            geneHistoryCanvas.MarkDirtyRepaint(); 
        });
    }

    void OnDisable()
    {
        populationChartCanvas.generateVisualContent -= DrawPopulationChart;
        geneHistoryCanvas.generateVisualContent -= DrawGeneHistoryChart;

        Statistics.OnGeneStatisticsUpdated -= UpdateGeneStatistics;
        Statistics.OnPopulationUpdated    -= UpdateOverallGeneCount;
        Statistics.OnPopulationUpdated    -= populationChartCanvas.MarkDirtyRepaint;
    }

#endregion

#region Common UI
    void ToggleHistory(ChangeEvent<bool> evt)
    {
        if(evt.newValue == true)
        {
            geneDistributionRoot.style.display = DisplayStyle.None;
            geneHistoryRoot.style.display = DisplayStyle.Flex;
            secondGeneTop.style.display = DisplayStyle.Flex;

            showGeneHistory = true;
        }
        else
        {
            geneDistributionRoot.style.display = DisplayStyle.Flex;
            geneHistoryRoot.style.display = DisplayStyle.None;
            secondGeneTop.style.display = DisplayStyle.None;

            showGeneHistory = false;
        }
    }

    void ChangeMode(ChangeEvent<Enum> evt)
    {
        if((StatisticsDisplayMode)evt.newValue == StatisticsDisplayMode.Gene)
        {
            geneHistogramRoot.style.display = DisplayStyle.Flex;
            populationChartRoot.style.display = DisplayStyle.None;
        }
        else if((StatisticsDisplayMode)evt.newValue == StatisticsDisplayMode.Population)
        {
            geneHistogramRoot.style.display = DisplayStyle.None;
            populationChartRoot.style.display = DisplayStyle.Flex;
        }
    }

    void PopulateGeneDropdown()
    {
        string previous = geneDropdown.value;
        var genes = Statistics.GetAllGenesNames();
        geneDropdown.choices = genes;
        geneDropdown.SetValueWithoutNotify(genes.Contains(previous) ? previous : genes[0]);

        chosenGene = geneDropdown.value;
    }

    void OnGeneDropdownChanged(ChangeEvent<string> evt)
    {
        chosenGene = evt.newValue;

        if(evt.newValue != evt.previousValue)
        {
            UpdateGeneStatistics(chosenGene);
        }
    }

    void UpdateOverallGeneCount()
    {
        geneOverallCountLabel.text = $"N — {Statistics.GetCurrentPopulation(chosenCreature)}";
    }
#endregion

#region Gene Histogram
    void UpdateGeneStatistics(string geneName)
    {
        PopulateGeneDropdown();

        if(chosenGene != geneName) { return; }

        float[] recordValues = Statistics.GetGeneRecordsAsArray(geneName);

        if (showGeneHistory)
        {
            float avg = recordValues.Average();
            geneAverageLabel.text = $"Avg: {avg:F2}";
            geneHistoryCanvas.MarkDirtyRepaint();
        }
        else
        {
            //prevents division by 0
            if(recordValues.Length < 2){ return; }

            float minVal = float.MaxValue, maxVal = float.MinValue, sum = 0f;
            foreach (var value in recordValues)
            {
                if (value < minVal) minVal = value;
                if (value > maxVal) maxVal = value;
                sum += value;
            }

            float avg = sum / (float)recordValues.Length;
            geneAverageLabel.text = $"Avg: {avg:F2}";

            RedrawHistogram(recordValues, minVal, maxVal);
        }
    }

    void RedrawHistogram(float[] input, float minValue, float maxValue)
    {
        geneDistributionCanvas.Clear();

        int valueCount = input.Length;
        int bins = 10;

        if(valueCount < bins)
        {
            bins = valueCount;
        }
        int[] counts = new int[bins];

        float interval = (maxValue - minValue) / bins;

        foreach (var value in input)
        {
            int b = Mathf.Clamp(Mathf.FloorToInt((value - minValue) / interval),0,bins-1);
            counts[b]++;
        }

        int maxCount = counts.Max();
        if (maxCount == 0) return;

        for (int i = 0; i < bins; i++)
        {
            float frac = counts[i] / (float)maxCount;

            VisualElement bar = new VisualElement();
            bar.AddToClassList("bar");
            bar.style.height = new StyleLength(new Length(frac * 100f, LengthUnit.Percent));
            bar.tooltip = $"[{minValue + i * interval:F3} – {minValue + (i + 1) * interval:F3}] N = {counts[i]}";
            bar.RegisterCallback<PointerEnterEvent>(ShowGeneTooptip);
            bar.RegisterCallback<PointerLeaveEvent>(HideGeneTooptip);

            geneDistributionCanvas.Add(bar);
        }
    }

    void ShowGeneTooptip(PointerEnterEvent evt)
    {
        VisualElement bar = (VisualElement)evt.currentTarget;
        geneTooltipLabel.text = bar.tooltip;
    }

    void HideGeneTooptip(PointerLeaveEvent evt)
    {
        geneTooltipLabel.text = "";
    }
#endregion

#region Population Chart
    void DrawPopulationChart(MeshGenerationContext ctx)
    {
        SortedDictionary<float,int> history = Statistics.GetPopulationHistory(chosenCreature);
        
        float w = populationChartCanvas.resolvedStyle.width;
        float h = populationChartCanvas.resolvedStyle.height;
        if (w <= 0 || h <= 0 || history.Count < 2) { return; }

        var chopedHistory = NumberOfDataToShow == -1 ? history : history.Skip(Mathf.Max(0, history.Count - NumberOfDataToShow));

        float minTime = chopedHistory.First().Key;
        float maxTime = chopedHistory.Last().Key;

        int minValue = chopedHistory.Min(x => x.Value);
        int maxValue = chopedHistory.Max(x => x.Value);

        var painter = ctx.painter2D;
        
        //draw line for current value
        painter.strokeColor = lineColor;
        painter.lineWidth   = 1f;
        painter.SetDashPattern(8f,5f);
        painter.BeginPath();
        painter.MoveTo(new Vector2(0, h - Mathf.InverseLerp(minValue, maxValue, chopedHistory.Last().Value) * h));
        painter.LineTo(new Vector2(w, h - Mathf.InverseLerp(minValue, maxValue, chopedHistory.Last().Value) * h));
        painter.Stroke();
        painter.SetDashPattern(null);
        //draw end

        //draw circle on current value
        painter.strokeColor = Color.white;
        painter.fillColor = lineColor;
        painter.lineWidth = 2f;
        painter.BeginPath();
        painter.Arc(new Vector2(w, h - Mathf.InverseLerp(minValue, maxValue, chopedHistory.Last().Value) * h), 6f, 0f, 360f);
        painter.Fill();
        painter.Stroke();
        //end draw

        //draw main chart line
        painter.strokeColor = lineColor;
        painter.lineWidth   = lineWidth;
        painter.lineCap     = LineCap.Round;
        painter.lineJoin    = LineJoin.Round;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0,h - Mathf.InverseLerp(minValue,maxValue,chopedHistory.First().Value) * h));

        int counter = 0;
        foreach(var pair in chopedHistory)
        {
            //this way of calculating x makes point evenly spread throughout the graph, which makes it look better
            float x = (chopedHistory.Count() > 1) ? counter * w / (chopedHistory.Count() - 1): 0;
            float y = h - Mathf.InverseLerp(minValue, maxValue,pair.Value) * h;
            
            painter.LineTo(new Vector2(x, y));
            counter++;
        }
        painter.Stroke();
        //draw end

        //draw horizontal cursor line
        if (cursorLocalPosition != Vector2.zero)
        {
            float step = w / (chopedHistory.Count() - 1);

            // Snap cursor X to nearest discrete column index
            int snappedIndex = Mathf.RoundToInt(cursorLocalPosition.x / step);
            snappedIndex = Mathf.Clamp(snappedIndex, 0, chopedHistory.Count() - 1);
            float snappedX = snappedIndex * step;

            var keyValuePair = chopedHistory.ElementAt(snappedIndex); 
            float snappedY = h - Mathf.InverseLerp(minValue, maxValue, keyValuePair.Value) * h;

            populationChartCanvas.tooltip = $"{keyValuePair.Key:F2}s - {keyValuePair.Value}";
            // Vertical cursor line
            painter.strokeColor = Color.lightGray;
            painter.lineWidth = 1f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(snappedX, h));
            painter.LineTo(new Vector2(snappedX, snappedY));
            painter.Stroke();

            // Circle fill (line color)
            painter.fillColor = lineColor;
            painter.BeginPath();
            painter.Arc(new Vector2(snappedX, snappedY), 4f, 0f, 360f);
            painter.Fill();
        }
        //end draw

        populationChartCanvas.schedule.Execute(() => BuildTimeAxisLabels(minTime, maxTime, populationXAxis));
        populationChartCanvas.schedule.Execute(() => BuildValueAxisLabels(minValue, maxValue, populationYAxis));
    }

    void BuildValueAxisLabels(float minPopulation, float maxPopulation, VisualElement canvas)
    {
        const float MIN_GAP   = 30f;
        const float FADE_ZONE = 10f;
        float chartH  = populationChartCanvas.resolvedStyle.height;
        float range   = maxPopulation - minPopulation;
        float pxPerUnit = chartH / Mathf.Max(range, 1);

        valueInterval = ValueInterval(range, pxPerUnit, MIN_GAP, valueInterval);

        float Normalize(float v)    => (v - minPopulation) / Mathf.Max(range, 1);
        float EdgeAlpha(float norm)
        {
            float fromEdge = Mathf.Min(norm, 1f - norm) * chartH;  // screen: y=0 is top
            if (fromEdge >= FADE_ZONE) return 1f;
            if (fromEdge <= 0f)        return 0f;
            return fromEdge / FADE_ZONE;
        }

        float ValueInterval(float valRange, float pxPerUnit, float minGap, float prev)
        {
            if (prev > 0)
            {
                float px = prev * pxPerUnit;
                if (px >= minGap * 0.5 && px <= minGap * 4)
                    return prev;
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

        var targets = new HashSet<int>();
        int first = Mathf.CeilToInt((float)(minPopulation - valueInterval) / valueInterval) * (int)valueInterval;
        for (int t = first; t <= maxPopulation + valueInterval && targets.Count < MAX_LABELS; t += (int)valueInterval)
        {
            targets.Add(t);
        }

        void DrawLabel(VisualElement container, float norm, float alpha, string text)
        {
            var lbl = new Label(text);
            lbl.AddToClassList("axis-text");
            lbl.style.opacity   = alpha;
            lbl.style.position  = Position.Absolute;
            lbl.style.top       = (1f - norm) * chartH;  // flip: value=1 → bottom of chart
            lbl.style.translate = new StyleTranslate(new Translate(0, Length.Percent(-50)));
            container.Add(lbl);
        }

        string FormatAxisValue(float value)
        {
            return value >= 1000 ? $"{value / 1000f:F1}k" : Mathf.Approximately(value, Mathf.Round(value)) ? ((int)Mathf.Round(value)).ToString() : value.ToString("F1");
        }

        var cfg = new AxisConfig(
            minGap:    MIN_GAP,
            fadeZone:  FADE_ZONE,
            chartSize: chartH,
            minValue:  minPopulation,
            maxValue:  maxPopulation,
            labelData: populationValueLabeldata,
            normalize: v   => Normalize(v),
            formatKey: key => FormatAxisValue(key),
            keyToData: key => key,              // Y-axis: key IS the data value
            edgeAlpha: Normalize => EdgeAlpha(Normalize),
            targets:   targets,
            drawLabel: DrawLabel);

        BuildAxisLabels(cfg, canvas);
    }
    void BuildTimeAxisLabels(float minTime, float maxTime, VisualElement canvas)
    {
        const float MIN_GAP   = 45f;
        const float FADE_ZONE = 50f;
        float chartW   = canvas.resolvedStyle.width;
        float timeSpan = maxTime - minTime;
        float interval = TimeInterval(timeSpan);
        float pxPerSec = chartW / timeSpan;

        while (interval * pxPerSec < MIN_GAP && interval < timeSpan)
            interval *= 2f;

        float Normalize(float t)    => (t - minTime) / timeSpan;
        float EdgeAlpha(float norm)
        {
            float fromEdge = Mathf.Min(norm, 1f - norm) * chartW;
            if (fromEdge >= FADE_ZONE) return 1f;
            if (fromEdge <= 0f)        return 0f;
            return fromEdge / FADE_ZONE;
        }

        static float TimeInterval(float spanSecs)
        {
            float[] steps = { 1, 2, 5, 10, 15, 30, 60, 120, 300, 600, 1800, 3600, 7200, 21600, 86400 };
            float target  = spanSecs / 6f;
            foreach (float state in steps)
            {
                if (state >= target) return state;
            }
            return steps[^1];
        }

        var targets = new HashSet<int>();
        float first = Mathf.Ceil((minTime - interval) / interval) * interval;
        for (float t = first; t <= maxTime + interval && targets.Count < MAX_LABELS; t += interval)
            targets.Add(Mathf.RoundToInt(t * 100));      // key = time * 100

        void DrawLabel(VisualElement container, float norm, float alpha, string text)
        {
            var lbl = new Label(text);
            lbl.AddToClassList("axis-text");
            lbl.style.opacity   = alpha;
            lbl.style.position  = Position.Absolute;
            lbl.style.left      = norm * chartW;
            lbl.style.translate = new StyleTranslate(new Translate(Length.Percent(-50), 0));
            container.Add(lbl);
        }

        var cfg = new AxisConfig(
            minGap:    MIN_GAP,
            fadeZone:  FADE_ZONE,
            chartSize: chartW,
            minValue:  minTime,
            maxValue:  maxTime,
            labelData: populationTimeLabelData,
            normalize: v   => Normalize(v),
            formatKey: key => $"{key / 100f:F0}s",
            keyToData: key => key / 100f,               // X-axis: key = time * 100
            edgeAlpha: norm => EdgeAlpha(norm),
            targets:   targets,
            drawLabel: DrawLabel);

        BuildAxisLabels(cfg, canvas);
    }
    void BuildAxisLabels(AxisConfig cfg, VisualElement container)
    {
        // 1. Sync label-state dict with this frame's targets
        foreach (int key in cfg.Targets)
        {
            string text = cfg.FormatKey(key);
            if (!cfg.LabelData.TryGetValue(key, out var state))
                cfg.LabelData[key] = (0f, text);
            else
            {
                state.text = text;
                cfg.LabelData[key] = state;
            }
        }

        // 2. Fade alphas toward target; prune fully-faded labels
        foreach (int key in cfg.LabelData.Keys.ToList())
        {
            var state  = cfg.LabelData[key];
            float norm = cfg.Normalize(cfg.KeyToData(key));
            float target = cfg.Targets.Contains(key) ? cfg.EdgeAlpha(norm) : 0f;

            float next = Mathf.Lerp(state.alpha, target, LABEL_FADE);
            if (Mathf.Abs(next - target) < 0.02f) next = target;

            if (next < 0.01f && target == 0f) { cfg.LabelData.Remove(key); continue; }

            state.alpha = next;
            cfg.LabelData[key] = state;
        }

        // 3. Collect labels that are visible and within range
        var visible = new List<(float pos, float alpha, string text)>();
        foreach (var (key, state) in cfg.LabelData)
        {
            if (state.alpha < 0.02f) continue;

            float pos = cfg.Normalize(cfg.KeyToData(key));
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
                float gap  = Mathf.Abs(label.pos - prev.pos) * cfg.ChartSize;
                if (gap < cfg.MinGap)
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
            cfg.DrawLabel(container, pos, alpha, text);
        }

    }
#endregion

#region Gene History Chart
    void DrawGeneHistoryChart(MeshGenerationContext ctx)
    {
        SortedDictionary<float,float> history = Statistics.GetGeneAvarageHistory(chosenGene);
        
        float w = geneHistoryCanvas.resolvedStyle.width;
        float h = geneHistoryCanvas.resolvedStyle.height;
        if (w <= 0 || h <= 0 || history.Count < 2) { return; }

        var chopedHistory = NumberOfDataToShow == -1 ? history : history.Skip(Mathf.Max(0, history.Count - NumberOfDataToShow));

        float minTime = chopedHistory.First().Key;
        float maxTime = chopedHistory.Last().Key;

        float minValue = chopedHistory.Min(x => x.Value);
        float maxValue = chopedHistory.Max(x => x.Value);

        var painter = ctx.painter2D;

        //draw line for current value
        painter.strokeColor = lineColor;
        painter.lineWidth   = 1f;
        painter.SetDashPattern(8f,5f);
        painter.BeginPath();
        painter.MoveTo(new Vector2(0, h - Mathf.InverseLerp(minValue, maxValue, chopedHistory.Last().Value) * h));
        painter.LineTo(new Vector2(w, h - Mathf.InverseLerp(minValue, maxValue, chopedHistory.Last().Value) * h));
        painter.Stroke();
        painter.SetDashPattern(null);
        //draw end

        //draw circle on current value
        painter.strokeColor = Color.white;
        painter.fillColor = lineColor;
        painter.lineWidth = 2f;
        painter.BeginPath();
        painter.Arc(new Vector2(w, h - Mathf.InverseLerp(minValue, maxValue, chopedHistory.Last().Value) * h), 6f, 0f, 360f);
        painter.Fill();
        painter.Stroke();
        //end draw

        //draw main chart line
        painter.strokeColor = lineColor;
        painter.lineWidth   = lineWidth;
        painter.lineCap     = LineCap.Round;
        painter.lineJoin    = LineJoin.Round;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0,h - Mathf.InverseLerp(minValue,maxValue,chopedHistory.First().Value) * h));

        int counter = 0;
        foreach(var pair in chopedHistory)
        {
            //this way of calculating x makes point evenly spread throughout the graph, which makes it look better
            float x = (chopedHistory.Count() > 1) ? counter * w / (chopedHistory.Count() - 1): 0;
            float y = h - Mathf.InverseLerp(minValue, maxValue,pair.Value) * h;
            
            painter.LineTo(new Vector2(x, y));
            counter++;
        }
        painter.Stroke();
        //draw end

        //draw horizontal cursor line
        if (cursorLocalPosition != Vector2.zero)
        {
            float step = w / (chopedHistory.Count() - 1);

            // Snap cursor X to nearest discrete column index
            int snappedIndex = Mathf.RoundToInt(cursorLocalPosition.x / step);
            snappedIndex = Mathf.Clamp(snappedIndex, 0, chopedHistory.Count() - 1);
            float snappedX = snappedIndex * step;

            var keyValuePair = chopedHistory.ElementAt(snappedIndex); 
            float snappedY = h - Mathf.InverseLerp(minValue, maxValue, keyValuePair.Value) * h;

            geneHistoryCanvas.tooltip = $"{keyValuePair.Key:F2}s - {keyValuePair.Value}";
            // Vertical cursor line
            painter.strokeColor = Color.lightGray;
            painter.lineWidth = 1f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(snappedX, h));
            painter.LineTo(new Vector2(snappedX, snappedY));
            painter.Stroke();

            // Circle fill (line color)
            painter.fillColor = lineColor;
            painter.BeginPath();
            painter.Arc(new Vector2(snappedX, snappedY), 4f, 0f, 360f);
            painter.Fill();
        }
        //end draw

        geneHistoryCanvas.schedule.Execute(() => BuildTimeAxisLabels(minTime, maxTime, geneXAxis));
        geneHistoryCanvas.schedule.Execute(() => BuildFloatValueAxisLabels(minValue, maxValue, geneYAxis));
    }

    void BuildFloatValueAxisLabels(float minValue, float maxValue, VisualElement canvas)
        {
        const float MIN_GAP   = 30f;
        const float FADE_ZONE = 10f;
        float chartH  = canvas.resolvedStyle.height;
        float range   = maxValue - minValue;
        float pxPerUnit = chartH / Mathf.Max(range, 0.0001f);

        valueInterval = ValueInterval(range, pxPerUnit, MIN_GAP, valueInterval);

        float Normalize(float v)    => (v - minValue) / Mathf.Max(range, 0.0001f);
        float EdgeAlpha(float norm)
        {
            float fromEdge = Mathf.Min(norm, 1f - norm) * chartH;
            if (fromEdge >= FADE_ZONE) return 1f;
            if (fromEdge <= 0f)        return 0f;
            return fromEdge / FADE_ZONE;
        }

        float ValueInterval(float valRange, float pxPerUnit, float minGap, float prev)
        {
            if (prev > 0)
            {
                float px = prev * pxPerUnit;
                if (px >= minGap * 0.5 && px <= minGap * 4)
                    return prev;
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

        // Scale factor: enough decimal places for float genes
        int scale = 10000;

        var targets = new HashSet<int>();
        float first = Mathf.Ceil((minValue - valueInterval) / valueInterval) * valueInterval;
        for (float t = first; t <= maxValue + valueInterval && targets.Count < MAX_LABELS; t += valueInterval)
        {
            targets.Add(Mathf.RoundToInt(t * scale));
        }

        void DrawLabel(VisualElement container, float norm, float alpha, string text)
        {
            var lbl = new Label(text);
            lbl.AddToClassList("axis-text");
            lbl.style.opacity   = alpha;
            lbl.style.position  = Position.Absolute;
            lbl.style.top       = (1f - norm) * chartH;
            lbl.style.translate = new StyleTranslate(new Translate(0, Length.Percent(-50)));
            container.Add(lbl);
        }

        string FormatAxisValue(float value)
        {
            return value.ToString("F2");
        }

        var cfg = new AxisConfig(
            minGap:    MIN_GAP,
            fadeZone:  FADE_ZONE,
            chartSize: chartH,
            minValue:  minValue,
            maxValue:  maxValue,
            labelData: geneValueLabeldata,
            normalize: v   => Normalize(v),
            formatKey: key => FormatAxisValue(key / (float)scale),
            keyToData: key => key / (float)scale,
            edgeAlpha: norm => EdgeAlpha(norm),
            targets:   targets,
            drawLabel: DrawLabel);

        BuildAxisLabels(cfg, canvas);
        }
#endregion
}

record AxisConfig
{
    // Layout
    public readonly float MinGap;
    public readonly float FadeZone;
    public readonly float ChartSize;        // width (X) or height (Y)

    // Data range
    public readonly float MinValue;
    public readonly float MaxValue;

    public readonly Dictionary<int, (float alpha, string text)> LabelData;

    // Delegates
    public readonly Func<float, float>  Normalize;     // dataValue → [0..1]
    public readonly Func<int, string>   FormatKey;     // dict key → label text
    public readonly Func<int, float>    KeyToData;     // dict key → data value
    public readonly Func<float, float>  EdgeAlpha;     // normalised pos → alpha
    public readonly HashSet<int>        Targets;       // keys for this frame
    public readonly Action<VisualElement, float, float, string> DrawLabel; // (container,normPos,alpha,text)
                                                       

    public AxisConfig(
        float minGap, float fadeZone, float chartSize,
        float minValue, float maxValue,
        Dictionary<int, (float alpha, string text)> labelData,
        Func<float, float>  normalize,
        Func<int, string>   formatKey,
        Func<int, float>    keyToData,
        Func<float, float>  edgeAlpha,
        HashSet<int>        targets,
        Action<VisualElement, float, float, string> drawLabel)
    {
        MinGap    = minGap;    FadeZone  = fadeZone; ChartSize = chartSize;
        MinValue  = minValue;  MaxValue  = maxValue; LabelData = labelData;
        Normalize = normalize; FormatKey = formatKey; KeyToData = keyToData;
        EdgeAlpha = edgeAlpha; Targets   = targets;   DrawLabel = drawLabel;
    }
}