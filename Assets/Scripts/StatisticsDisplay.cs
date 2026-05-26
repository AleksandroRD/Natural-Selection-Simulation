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

    private Dictionary<int, (float alpha, string text)> timeLabelData = new();
    private Dictionary<int, (float alpha, string text)> valueLabeldata = new();

    private const float LABEL_FADE = 0.08f;
    private const int   MAX_LABELS = 30;    // safety cap

    private float valueInterval;
    private Vector2 _cursorLocalPosition;

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

    VisualElement  populationChartCanvas;
    VisualElement  populationChartRoot;

    VisualElement populationXAxis;
    VisualElement populationYAxis;

    Button button50;
    Button button100;
    Button button250;
    Button button500;
    Button buttonAll;

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

        populationChartRoot = root.Q<VisualElement>("population-chart");
        populationChartCanvas = root.Q<VisualElement>("population-chart-canvas");
        populationXAxis = root.Q<VisualElement>("population-chart-axis-x");
        populationYAxis = root.Q<VisualElement>("population-chart-axis-y");

        button50 = root.Q<Button>("50dp-button");
        button100 = root.Q<Button>("100dp-button");
        button250 = root.Q<Button>("250dp-button");
        button500 = root.Q<Button>("500dp-button");
        buttonAll = root.Q<Button>("alldp-button");

        button50.clicked += () => {NumberOfDataToShow = 50; populationChartCanvas.MarkDirtyRepaint();};
        button100.clicked += () => {NumberOfDataToShow = 100; populationChartCanvas.MarkDirtyRepaint();};
        button250.clicked += () => {NumberOfDataToShow = 250; populationChartCanvas.MarkDirtyRepaint();};
        button500.clicked += () => {NumberOfDataToShow = 500; populationChartCanvas.MarkDirtyRepaint();};
        buttonAll.clicked += () => {NumberOfDataToShow = -1;};

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
            _cursorLocalPosition = evt.localPosition;
            populationChartCanvas.MarkDirtyRepaint(); 
        });

        populationChartCanvas.RegisterCallback<PointerOverEvent>((ctx) =>
        {
            UnityEngine.Cursor.SetCursor(precisionCursor, new Vector2(precisionCursor.width/2,precisionCursor.height/2),CursorMode.Auto);
        });

        populationChartCanvas.RegisterCallback<PointerOutEvent>((ctx) =>
        {
            _cursorLocalPosition = Vector3.zero;
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
            
            showGeneHistory = true;
        }
        else
        {
            geneDistributionRoot.style.display = DisplayStyle.Flex;
            geneHistoryRoot.style.display = DisplayStyle.None;

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

        if (showGeneHistory)
        {
            geneHistoryCanvas.MarkDirtyRepaint();
        }
        else
        {
            float[] recordValues = Statistics.GetGeneRecordsAsArray(geneName);
            int valueCount = recordValues.Length;

            //prevents division by 0
            if(valueCount < 2){ return; }

            float minVal = float.MaxValue, maxVal = float.MinValue, sum = 0f;
            foreach (var value in recordValues)
            {
                if (value < minVal) minVal = value;
                if (value > maxVal) maxVal = value;
                sum += value;
            }

            float avg = sum / (float)valueCount;
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
        if (w <= 0 || h <= 0 || history.Count < 2) return;

        var chopedHistory = NumberOfDataToShow == -1 ? history : history.Skip(Mathf.Max(0, history.Count - NumberOfDataToShow));

        float minTime = chopedHistory.First().Key;
        float maxTime = chopedHistory.Last().Key;

        int minValue = chopedHistory.Min(x => x.Value);
        int maxValue = chopedHistory.Max(x => x.Value);

        var painter = ctx.painter2D;
        
        //draw line for current value
        painter.strokeColor = Color.lightGray;
        painter.lineWidth   = 1f;
        painter.SetDashPattern(8f,5f);
        painter.BeginPath();
        painter.MoveTo(new Vector2(0, h - Mathf.InverseLerp(minValue, maxValue, chopedHistory.Last().Value) * h));
        painter.LineTo(new Vector2(w, h - Mathf.InverseLerp(minValue, maxValue, chopedHistory.Last().Value) * h));
        painter.Stroke();
        painter.SetDashPattern(null);
        //draw end
        
        //draw main chart line
        painter.lineWidth   = lineWidth;
        painter.lineJoin    = LineJoin.Round;
        painter.strokeColor = lineColor;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0,h - Mathf.InverseLerp(minValue,maxValue,chopedHistory.First().Value) * h));

        int counter = 0;
        foreach(var pair in chopedHistory)
        {
            //this way of calculating x makes point evenly spread throughout the graph, which makes it look better
            float x = counter * w / chopedHistory.Count();
            float y = h - Mathf.InverseLerp(minValue, maxValue,pair.Value) * h;
            
            painter.LineTo(new Vector2(x, y));
            counter++;
        }
        painter.Stroke();
        //draw end

        //draw horizontal cursor line
        if(_cursorLocalPosition != Vector2.zero)
        { 
            painter.strokeColor = Color.lightGray;
            painter.lineWidth = 1f;
            painter.BeginPath();
            //clamp x to descrete values
            float x  = Mathf.RoundToInt(_cursorLocalPosition.x / (w / chopedHistory.Count())) * (w / chopedHistory.Count());
            
            painter.MoveTo(new Vector2(x,h));
            painter.LineTo(new Vector2(x,0));
            painter.Stroke();
        }
        //end draw

        populationChartCanvas.schedule.Execute(() => BuildTimeAxisLabels(minTime,maxTime, Time.deltaTime));
        populationChartCanvas.schedule.Execute(() => BuildValueAxisLabels(minValue, maxValue));
    }

    void BuildValueAxisLabels(int minPopulation, int maxPopulation)
    {
        const float MIN_GAP = 30f;
        const float FADE_ZONE  = 25f;

        float chartH = populationChartCanvas.resolvedStyle.height;

        int valueRange = maxPopulation - minPopulation;
        float pxPerUnit = chartH / Mathf.Max(valueRange, 1);
        valueInterval = PickInterval(valueRange, pxPerUnit, MIN_GAP, valueInterval);
        while (valueInterval * pxPerUnit < 30f && valueInterval < valueRange)
        {
            valueInterval *= 2;
        }

        float NormalizeValue(int v) => (v - minPopulation) / (float)Mathf.Max(valueRange, 1);

        float EdgeAlpha(float y)
        {      
            float fromTop   = (1f - y) * chartH; // Y=0 is top in screen space
            float fromBot   = chartH - fromTop;
            float fromEdge  = Mathf.Min(fromTop, fromBot);

            if (fromEdge >= FADE_ZONE) return 1f;
            if (fromEdge <= 0f)        return 0f;

            return fromEdge / FADE_ZONE;
        }

        // Generate labels: current view + 1 interval buffer.
        var targets = new HashSet<int>();
        int first = Mathf.CeilToInt((float)(minPopulation - valueInterval) / valueInterval) * (int)valueInterval;
        for (int t = first; t <= maxPopulation + valueInterval && targets.Count < MAX_LABELS; t += (int)valueInterval)
        {
            targets.Add(t);
        }

        foreach (int key in targets)
        {
            string text = FormatAxisValue((int)key);
            if (!valueLabeldata.TryGetValue(key, out var state))
            {
                
                valueLabeldata[key] = (0f, text );
            }
            else 
            { 
                state.text = text; valueLabeldata[key] = state; 
            }
        }

        foreach (var key in valueLabeldata.Keys.ToList())
        {
            var state = valueLabeldata[key];
            float target = targets.Contains(key) ? EdgeAlpha(NormalizeValue(key)) : 0f;

            float next = Mathf.Lerp(state.alpha, target, LABEL_FADE);

            if (Mathf.Abs(next - target) < 0.02f) { next = target; }

            if (next < 0.01f && target == 0f) { valueLabeldata.Remove(key); continue; }

            state.alpha = next;
            valueLabeldata[key] = state;
        }

        var visibleLabels = new List<(float value, float alpha, string text)>();
        foreach (var (key, state) in valueLabeldata)
        {
            if (state.alpha < 0.02f) continue;

            float value = NormalizeValue(key);

            if (value < -0.05f || value > 1.05f) continue;

            visibleLabels.Add((value, state.alpha, state.text));
        }
        visibleLabels.Sort((a, b) => b.value.CompareTo(a.value)); // top→bottom

        var drawnLabels = new List<(float value, float alpha, string text)>();
        foreach (var label in visibleLabels)
        {
            if (drawnLabels.Count > 0)
            {
                var prev    = drawnLabels[^1];
                float gapPx = Mathf.Abs(label.value - prev.value) * chartH;
                if (gapPx < MIN_GAP)
                {
                    if (label.alpha > prev.alpha) drawnLabels[^1] = label;
                    continue;
                }
            }
            drawnLabels.Add(label);
        }

        populationYAxis.Clear();
        foreach (var (value, alpha, text) in drawnLabels)
        {
            var lbl = new Label(text);
            lbl.AddToClassList("axis-text");
            lbl.style.opacity  = alpha;
            lbl.style.position = Position.Absolute;
            // value=1 → bottom of chart → top offset = (1-value)*chartH = 0... flip:
            lbl.style.top       = (1f - value) * chartH;
            lbl.style.translate = new StyleTranslate(new Translate(0, Length.Percent(-50)));
            populationYAxis.Add(lbl);
        }
    }           

    void BuildTimeAxisLabels(float minTime, float maxTime, float dt)           
    {
        const float MIN_GAP = 45f;   // px gap between label centres
        const float FADE_ZONE  = 50f;

        float chartW = populationChartCanvas.resolvedStyle.width;

        float timeSpan = maxTime - minTime;
        float interval = NiceTimeInterval(timeSpan);
        float pxPerSec = chartW / timeSpan;

        // Widen interval until labels are at least MIN_GAP px apart
        while (interval * pxPerSec < MIN_GAP && interval < timeSpan)
        {
            interval *= 2f;
        }

        // Convert time → normalised X position [0..1]
        float NormalizeTime(float t) => (t - minTime) / timeSpan;

        float EdgeAlpha(float x)
        {    
            float fromLeft  = x * chartW;
            float fromRight = chartW - fromLeft;
            float fromEdge  = Mathf.Min(fromLeft, fromRight);

            if (fromEdge >= FADE_ZONE) return 1f;
            if (fromEdge <= 0f)        return 0f;

            return fromEdge / FADE_ZONE;
        }

        // Build the set of target timestamps (rounded to avoid float keys)
        var targets = new HashSet<int>();
        float first = Mathf.Ceil((minTime - interval) / interval) * interval;

        for (float t = first; t <= maxTime + interval && targets.Count < MAX_LABELS; t += interval)
        {
            targets.Add(Mathf.RoundToInt(t * 100));  // key = time * 100
        }

        // Create / update label states
        foreach (int key in targets)
        {
            string text = $"{key / 100f:F0}s";
            if (!timeLabelData.TryGetValue(key, out var state))
                timeLabelData[key] = (0f, text);
            else
            {
                state.text = text;
                timeLabelData[key] = state;
            }
        }

        // Update alphas
        foreach (var key in timeLabelData.Keys.ToList())
        {
            var label = timeLabelData[key];
            float target = targets.Contains(key) ? EdgeAlpha(NormalizeTime(key / 100f)) : 0f;

            float next = Mathf.Lerp(label.alpha, target, LABEL_FADE);
            if (Mathf.Abs(next - target) < 0.02f) { next = target; }

            if (next < 0.01f && target == 0f) { timeLabelData.Remove(key); continue; }

            timeLabelData[key] = (next, label.text);
        }

        // Collect visible labels, sort by position
        var xVisible = new List<(float time, float alpha, string text)>();
        foreach (var (key, state) in timeLabelData)
        {
            if (state.alpha < 0.02f) continue;

            float time = NormalizeTime(key / 100f);

            if (time < -0.05f || time > 1.05f) continue;

            xVisible.Add((time, state.alpha, state.text));
        }
        xVisible.Sort((a, b) => a.time.CompareTo(b.time));

        // Overlap resolution — keep the higher-alpha label when two collide
        var drawnLabels = new List<(float time, float alpha, string text)>();
        foreach (var label in xVisible)
        {
            if (drawnLabels.Count > 0)
            {
                var prev    = drawnLabels[^1];
                float gap = (label.time - prev.time) * chartW;
                if (gap < MIN_GAP)
                {
                    if (label.alpha > prev.alpha)
                        drawnLabels[^1] = label;   // swap in the more-visible one
                    continue;
                }
            }
            drawnLabels.Add(label);
        }

        // Write UI labels
        populationXAxis.Clear();
        foreach (var (time, alpha, text) in drawnLabels)
        {
            var lbl = new Label(text);
            lbl.AddToClassList("axis-text");
            lbl.style.opacity = alpha;
            // Position absolutely so overlap removal has meaning
            lbl.style.position = Position.Absolute;
            lbl.style.left     = time * chartW;
            lbl.style.translate = new StyleTranslate(new Translate(Length.Percent(-50), 0));
            populationXAxis.Add(lbl);
        }
  
    }

    static float NiceTimeInterval(float spanSecs)
    {
        float[] steps = { 1, 2, 5, 10, 15, 30, 60, 120, 300, 600, 1800, 3600, 7200, 21600, 86400 };
        float target  = spanSecs / 6f;
        foreach (float state in steps)
        {
            if (state >= target) return state;
        }
        return steps[^1];
    }

    public static float PickInterval(float valRange, float pxPerUnit, float minGap, float prev)
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

    string FormatAxisValue(float value)
    {
        return value >= 1000 ? $"{value / 1000f:F1}k" : Mathf.Approximately(value, Mathf.Round(value)) ? ((int)Mathf.Round(value)).ToString() : value.ToString("F1");
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

        painter.strokeColor = lineColor;
        painter.lineWidth   = lineWidth;
        painter.lineCap     = LineCap.Round;
        painter.lineJoin    = LineJoin.Round;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0,h - Mathf.InverseLerp(minValue,maxValue,chopedHistory.First().Value) * h));

        int counter = 0;
        foreach (var pair in chopedHistory)
        {
            //this way of calculating x makes point evenly spread throughout the graph, which makes it look better
            float x = counter * w / chopedHistory.Count();
            float y = h - Mathf.InverseLerp(minValue, maxValue,pair.Value) * h;
            painter.LineTo(new Vector2(x, y));
            counter++;
        }
        
        painter.Stroke();
    }
#endregion
}