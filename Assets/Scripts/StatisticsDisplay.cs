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
    string chosenGene;

    string chosenCreature = "Rabbit";
    bool showGeneHistory = false;
    int NumberOfDataToShow = 100;
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
        buttonAll.clicked += () => {/*NumberOfDataToShow = -1;*/};//TODO: handle the all case

        geneDropdown.RegisterValueChangedCallback(OnGeneDropdownChanged);
        geneHistoryToggle.RegisterValueChangedCallback(ToggleHistory);
        modeDropdown.RegisterValueChangedCallback(ChangeMode);

        populationChartCanvas.generateVisualContent += DrawPopulationChart;
        geneHistoryCanvas.generateVisualContent += DrawGeneHistoryChart;
        Statistics.OnGeneStatisticsUpdated += UpdateGeneStatistics;
        Statistics.OnPopulationUpdated    += UpdateOverallGeneCount;
        Statistics.OnPopulationUpdated    += populationChartCanvas.MarkDirtyRepaint;
    }

    void OnDisable()
    {
        populationChartCanvas.generateVisualContent -= DrawPopulationChart;

        Statistics.OnGeneStatisticsUpdated -= UpdateGeneStatistics;
        Statistics.OnPopulationUpdated    -= UpdateOverallGeneCount;
        Statistics.OnPopulationUpdated    -= populationChartCanvas.MarkDirtyRepaint;
    }
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

    void DrawPopulationChart(MeshGenerationContext ctx)
    {
        SortedDictionary<float,float> history = Statistics.GetPopulationHistory(chosenCreature);
        
        float w = populationChartCanvas.resolvedStyle.width;
        float h = populationChartCanvas.resolvedStyle.height;
        if (w <= 0 || h <= 0 || history.Count < 2) return;

        int minValue = (int)history.Values.Min();
        int maxValue = (int)history.Values.Max();

        List<Vector2> points = new List<Vector2>();
        int counter = 1;
        
        int toSkip = Mathf.Max(0, history.Count - NumberOfDataToShow);
        int numberOfpoints = NumberOfDataToShow > history.Count ? history.Count : NumberOfDataToShow;

        var chopedHistory = history.Skip(toSkip);
        float minTime = chopedHistory.First().Key;
        float maxTime = chopedHistory.Last().Key;

        foreach(var pair in chopedHistory)
        {
            float x = counter * w / numberOfpoints;
            float y = h - (pair.Value / maxValue * h);
            points.Add(new Vector2(x,y));
            counter++;
        }

        var painter = ctx.painter2D;

        painter.strokeColor = lineColor;
        painter.lineWidth   = lineWidth;
        painter.lineCap     = LineCap.Round;
        painter.lineJoin    = LineJoin.Round;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0,points[0].y));
        foreach(var point in points)
        {
            painter.LineTo(point);
        }
        painter.Stroke();

        float maxVal = history.Values.Max();
        populationChartCanvas.schedule.Execute(() => BuildPopulationAxisLabels(minValue, maxValue, minTime,maxTime));
    }

    void BuildPopulationAxisLabels(int minPopulation, int maxPopulation, float minTime, float maxTime)
    {
        // ── Y-axis (population values, top = max, bottom = min) ───────────
        populationYAxis.Clear();
        int ySteps = 5;
        for (int i = ySteps; i >= 0; i--)
        {
            int value = Mathf.RoundToInt(Mathf.Lerp(minPopulation, maxPopulation, i / (float)ySteps));
            var lbl = new Label(FormatAxisValue(value));
            lbl.AddToClassList("axis-text");
            populationYAxis.Add(lbl);
        }
    
        // ── X-axis (time values, left = minTime, right = maxTime) ─────────
        populationXAxis.Clear();
        int xSteps = 5;
        for (int i = 0; i <= xSteps; i++)
        {
            float time = Mathf.Lerp(minTime, maxTime, i / (float)xSteps);
            var lbl = new Label($"{time:F0}s");
            lbl.AddToClassList("axis-text");
            populationXAxis.Add(lbl);
        }
    }

    string FormatAxisValue(float v)
    {
        return v >= 1000 ? $"{v / 1000f:F1}k" :
        Mathf.Approximately(v, Mathf.Round(v)) ? ((int)Mathf.Round(v)).ToString() : v.ToString("F1");
    }

    void DrawGeneHistoryChart(MeshGenerationContext ctx)
    {
        SortedDictionary<float,float> history = Statistics.GetGeneAvarageHistory(chosenGene);
        
        float w = geneHistoryCanvas.resolvedStyle.width;
        float h = geneHistoryCanvas.resolvedStyle.height;
        if (w <= 0 || h <= 0 || history.Count < 2) { return; }

        float maxValue = history.Values.Max();

        List<Vector2> points = new List<Vector2>();
        int counter = 1;
        int NumberOfDataToShow = 100;
        int toSkip = Mathf.Max(0, history.Count - NumberOfDataToShow);
        int numberOfpoints = NumberOfDataToShow > history.Count ? history.Count : NumberOfDataToShow;

        foreach (var pair in history.Skip(toSkip))
        {
            float x = counter * w / numberOfpoints;
            float y = h - (pair.Value / maxValue * h);
            points.Add(new Vector2(x, y));
            counter++;
        }

        var painter = ctx.painter2D;

        painter.strokeColor = lineColor;
        painter.lineWidth   = lineWidth;
        painter.lineCap     = LineCap.Round;
        painter.lineJoin    = LineJoin.Round;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0,points[0].y));
        for(int i = 1; i < points.Count; i++)
        {
            painter.LineTo(points[i]);
        }
        painter.Stroke();

        foreach (var pt in points)
        {
            // coloured centre
            painter.fillColor = Color.black;
            painter.BeginPath();
            painter.Arc(pt, 2.5f * (1 - numberOfpoints / NumberOfDataToShow), 0, 360);
            painter.Fill();
        }
    }
}