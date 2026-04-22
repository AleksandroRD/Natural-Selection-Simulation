using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum StatisticsDispalyMode
{
    Population,
    Gene
}

[RequireComponent(typeof(UIDocument))]
public class StatisticsDispaly : MonoBehaviour
{
    [SerializeField] private Color lineColor   = new Color(0.27f, 0.71f, 1f);
    [SerializeField] private float lineWidth   = 2.5f;
    string chosenGene = "Speed Gene";

    string chosenCreature = "Rabbit";

    VisualElement  geneHistogramCanvas;
    DropdownField  geneDropdown;
    Label          geneAverageLabel;
    Label          geneOverallCountLabel;

    VisualElement  populationChartCanvas;
    EnumField      modeDropdown;
    VisualElement  geneHistogramRoot;
    VisualElement  populationChartRoot;
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        modeDropdown = root.Q<EnumField>("mode-dropdown");
        geneHistogramRoot = root.Q<VisualElement>("gene-chart");
        populationChartRoot = root.Q<VisualElement>("population-chart");
        geneHistogramCanvas    = root.Q<VisualElement>("gene-histogram-canvas");
        geneAverageLabel = root.Q<Label>("gene-average-label");
        geneDropdown = root.Q<DropdownField>("gene-dropdown");
        geneOverallCountLabel = root.Q<Label>("gene-overall-count-label");
        populationChartCanvas = root.Q<VisualElement>("population-chart-canvas");

        modeDropdown.RegisterValueChangedCallback(OnModeDropdownChange);
        geneDropdown.RegisterValueChangedCallback(OnGeneDropdownChanged);

        populationChartCanvas.generateVisualContent += DrawPopulationChart;

        ChangeMode();
    }

    void ChangeMode()
    {
        if((StatisticsDispalyMode)modeDropdown.value == StatisticsDispalyMode.Gene)
        {
            geneHistogramRoot.style.display = DisplayStyle.Flex;
            populationChartRoot.style.display  = DisplayStyle.None;
            Statistics.OnPopulationUpdated -= populationChartCanvas.MarkDirtyRepaint;

            Statistics.OnGeneStatisticsUpdated += UpdateGeneHistogram;
            Statistics.OnPopulationUpdated += UpdateOverallGeneCount;
        }
        else
        {
            geneHistogramRoot.style.display = DisplayStyle.None;
            populationChartRoot.style.display  = DisplayStyle.Flex;

            Statistics.OnPopulationUpdated += populationChartCanvas.MarkDirtyRepaint;

            Statistics.OnGeneStatisticsUpdated -= UpdateGeneHistogram;
            Statistics.OnPopulationUpdated -= UpdateOverallGeneCount;
        }

    }
    void OnModeDropdownChange(ChangeEvent<Enum> evt)
    {
        ChangeMode();
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
            UpdateGeneHistogram(chosenGene);
        }
    }
    void UpdateOverallGeneCount()
    {
        geneOverallCountLabel.text = "N - " + Statistics.GetCurrentPopulation("Rabbit").ToString();
    }

    void UpdateGeneHistogram(string geneName)
    {
        PopulateGeneDropdown();
        
        if(chosenGene != geneName) { return; }
        float[] recordValues = Statistics.GetGeneRecordsAsArray(geneName);
        int valueCount = recordValues.Count();

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
        geneAverageLabel.text = "Avg: " + avg;

        RedrawHistogram(recordValues, minVal, maxVal);
    }

    void RedrawHistogram(float[] input, float minValue, float maxValue)
    {
        geneHistogramCanvas.Clear();

        int valueCount = input.Count();
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
            bar.tooltip = $"[{minValue + i * interval:F2} – {maxValue + (i + 1) * interval:F2}]\nCount: {counts[i]}";

            geneHistogramCanvas.Add(bar);
        }
    }

    void DrawPopulationChart(MeshGenerationContext ctx)
    {
        SortedDictionary<float,float> history = Statistics.GetPopulationHistory(chosenCreature);
        
        float w = populationChartCanvas.resolvedStyle.width;
        float h = populationChartCanvas.resolvedStyle.height;
        if (w <= 0 || h <= 0 || history.Count < 2) return;

        float maxValue = history.Values.Max();

        List<Vector2> points = new List<Vector2>();
        int counter = 1;
        foreach(var pair in history)
        {
            float x = counter * w / history.Count;
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
        painter.MoveTo(new Vector2(0,h));
        foreach(var point in points)
        {
            painter.LineTo(point);
        }
        painter.Stroke();

        foreach (var pt in points)
        {
            // coloured centre
            painter.fillColor = Color.black;
            painter.BeginPath();
            painter.Arc(pt, 5f * 0.5f, 0, 360);
            painter.Fill();
        }

        
    }
}