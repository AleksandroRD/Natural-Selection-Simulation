using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Statistics : MonoBehaviour
{
    static readonly Dictionary<string, Dictionary<Guid, float>> geneRecords = new Dictionary<string, Dictionary<Guid, float>>();
    static readonly Dictionary<string, Dictionary<float, float>> geneHistoryAvarage = new Dictionary<string, Dictionary<float, float>>();
    static readonly Dictionary<string,int> population = new Dictionary<string, int>();
    static readonly Dictionary<string, Dictionary<float, float>> populationHistory = new Dictionary<string, Dictionary<float, float>>();

    public static event Action<string> OnGeneStatisticsUpdated;

    public static float[] GetGeneRecordsAsArray(string geneName)
    {
        return geneRecords[geneName].Values.ToArray();
    }

    public static void LogGene(string geneName, Guid id, float value)
    {
        if (!geneRecords.ContainsKey(geneName) || !geneHistoryAvarage.ContainsKey(geneName) )
        {
            geneRecords.Add(geneName, new Dictionary<Guid, float>());
            geneHistoryAvarage.Add(geneName, new Dictionary<float, float>());
        }

        geneRecords[geneName][id] = value;

        geneHistoryAvarage[geneName].Add(Time.realtimeSinceStartup, calculateAvarage(geneName));

        OnGeneStatisticsUpdated.Invoke(geneName);
    }

    public static void DelogGene(string geneName,Guid id)
    {
        geneRecords[geneName].Remove(id);
        geneHistoryAvarage[geneName].Add(Time.realtimeSinceStartup, calculateAvarage(geneName));
        
        OnGeneStatisticsUpdated.Invoke(geneName);
    }

    public static void LogPopulation(string creatureName, bool born)
    {
        if (!populationHistory.ContainsKey(creatureName) || !population.ContainsKey(creatureName))
        {
            populationHistory.Add(creatureName, new Dictionary<float, float>());
            population.Add(creatureName,0);
        }

        if (born)
        {
            population[creatureName]++;
        }
        else
        {
            population[creatureName]--;
        }

        populationHistory[creatureName].Add(Time.realtimeSinceStartup, population[creatureName]);
    }
    
    static float calculateAvarage(string geneName)
    {
        int count = geneRecords[geneName].Count;
        float sum = 0;
        foreach(var record in geneRecords[geneName])
        {
            sum += record.Value;
        }

        return sum / count;
    }
    
}