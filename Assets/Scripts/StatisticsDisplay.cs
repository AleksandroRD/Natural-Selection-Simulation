using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GUIUtils;
using System.Numerics;
using System.Collections.Generic;
public enum StatisticsDisplayMode
{
    Population,
    Gene
}

[RequireComponent(typeof(UIDocument))]
public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private Color lineColor   = new Color(0.27f, 0.71f, 1f);

    [SerializeField] private Texture2D precisionCursor;
    string chosenGene;

    string chosenCreature = "Rabbit";
    bool showGeneHistory = false;
    int NumberOfDataToShow = 100;
    
    LiveLineChart  populationChart;
    LiveLineChart  geneDestributionChart;
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
        geneDistributionCanvas = root.Q<VisualElement>("gene-distribution-canvas");
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

        ChartConfig populationConfig = new()
        {
            ChartCanvas = populationChartCanvas,
            TimeAxisCanvas = populationXAxis,
            ValueAxisCanvas = populationYAxis,
            lineColor = lineColor,
            FormatTime = value => $"{value : F0}s",
            FormatValue = value => $"{value : F2}",
            FadeZone = 30,
            LabelFade = 0.08f,
            minTimeLabelGaps = 30.0f,
            minValueLabelGaps = 20.0f,
            cursorTexture = precisionCursor,
        };
        populationChart = new LiveLineChart(populationConfig);

        ChartConfig geneConfig = new()
        {
            ChartCanvas = geneHistoryCanvas,
            TimeAxisCanvas = geneXAxis,
            ValueAxisCanvas = geneYAxis,
            lineColor = lineColor,
            FormatTime = value => $"{value : F0}s",
            FormatValue = value => $"{value : F2}",
            FadeZone = 30,
            LabelFade = 0.08f,
            minTimeLabelGaps = 30.0f,
            minValueLabelGaps = 20.0f,
            cursorTexture = precisionCursor,
        };
        geneDestributionChart = new LiveLineChart(geneConfig);

        geneDropdown.RegisterValueChangedCallback(OnGeneDropdownChanged);
        geneHistoryToggle.RegisterValueChangedCallback(ToggleHistory);
        modeDropdown.RegisterValueChangedCallback(ChangeMode);

        Statistics.OnGeneStatisticsUpdated += UpdateGeneStatistics;
        Statistics.OnPopulationUpdated    += UpdateOverallGeneCount;
        Statistics.OnPopulationUpdated    += UpdatePopulationChart;
    }

    void OnDisable()
    {
        Statistics.OnGeneStatisticsUpdated -= UpdateGeneStatistics;
        Statistics.OnPopulationUpdated    -= UpdateOverallGeneCount;
        Statistics.OnPopulationUpdated    -= UpdatePopulationChart;
    }

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

    void UpdateGeneStatistics(string geneName)
    {
        PopulateGeneDropdown();

        if(chosenGene != geneName) { return; }

        float[] recordValues = Statistics.GetGeneRecordsAsArray(geneName);

        if (showGeneHistory)
        {
            float avg = recordValues.Average();
            geneAverageLabel.text = $"Avg: {avg:F2}";
            geneDestributionChart.UpdateData(Statistics.GetGeneAvarageHistory(chosenGene));
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

    void UpdatePopulationChart()
    {
        var converted = new SortedDictionary<float, float>(
            Statistics.GetPopulationHistory(chosenCreature).ToDictionary(kvp => kvp.Key, kvp => (float)kvp.Value)
        );
        populationChart.UpdateData(converted);
    }
}
    
