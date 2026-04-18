using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(UIDocument))]
public class StatisticsDispaly : MonoBehaviour
{

    string chosenGene = "Speed Gene";
    VisualElement  _barCanvas;
    DropdownField  _geneDropdown;
    Label          _averageLabel;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _barCanvas    = root.Q<VisualElement>("bar-canvas");
        _averageLabel = root.Q<Label>("average-label");
        _geneDropdown = root.Q<DropdownField>("gene-dropdown");

        _geneDropdown.RegisterValueChangedCallback(OnDropdownChanged);
        Statistics.OnGeneStatisticsUpdated += UpdateStatistics;
    }

    void OnDropdownChanged(ChangeEvent<string> evt)
    {
        chosenGene = evt.newValue;

        if(evt.newValue != evt.previousValue)
        {
            UpdateStatistics(chosenGene);
        }
    }

    void UpdateStatistics(string geneName)
    {
        PopulateDropdown();
        
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
        _averageLabel.text = "Avg: " + avg;

        RedrawHistogram(recordValues, minVal, maxVal, avg);
    }

    void RedrawHistogram(float[] input, float minValue, float maxValue, float avg)
    {
        _barCanvas.Clear();

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

        int maxCount = 0;
        foreach (int c in counts) {
            if (c > maxCount) { maxCount = c; }
        }
        if (maxCount == 0) return;

        for (int i = 0; i < bins; i++)
        {
            float frac = counts[i] / (float)maxCount;

            VisualElement bar = new VisualElement();
            bar.AddToClassList("bar");
            bar.style.height = new StyleLength(new Length(frac * 100f, LengthUnit.Percent));
            bar.tooltip = $"[{minValue + i * interval:F2} – {maxValue + (i + 1) * interval:F2}]\nCount: {counts[i]}";

            _barCanvas.Add(bar);
        }
    }

    void PopulateDropdown()
    {
        string previous = _geneDropdown.value;
        var genes = Statistics.GetAllGenesNames();
        _geneDropdown.choices = genes;
        _geneDropdown.SetValueWithoutNotify(genes.Contains(previous) ? previous : genes[0]);

        chosenGene = _geneDropdown.value;
    }
}