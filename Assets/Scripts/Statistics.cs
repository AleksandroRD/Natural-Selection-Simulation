using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Statistics : MonoBehaviour
{
    static readonly Dictionary<string, Dictionary<Guid, float>> geneRecords = new Dictionary<string, Dictionary<Guid, float>>();
    static readonly Dictionary<string, SortedDictionary<float, float>> geneHistoryAvarage = new Dictionary<string, SortedDictionary<float, float>>();
    static readonly Dictionary<string,int> population = new Dictionary<string, int>();
    static readonly Dictionary<string, SortedDictionary<float, float>> populationHistory = new Dictionary<string, SortedDictionary<float, float>>();

    public static event Action<string> OnGeneStatisticsUpdated;
    public static event Action OnPopulationUpdated;

    public static float[] GetGeneRecordsAsArray(string geneName)
    {
        return geneRecords[geneName].Values.ToArray();
    }

    public static List<string> GetAllGenesNames()
    {
        return geneRecords.Keys.ToList();
    }

    public static int GetCurrentPopulation(string animalName)
    {
        if(!population.ContainsKey(animalName)) { return 0; }
        return population[animalName];
    }

    public static SortedDictionary<float, float> GetPopulationHistory(string animalName)
    {
        if(!populationHistory.ContainsKey(animalName)) { return null; }
        return populationHistory[animalName];
    }

    public static void LogGene(string geneName, Guid id, float value)
    {
        if (!geneRecords.ContainsKey(geneName) || !geneHistoryAvarage.ContainsKey(geneName) )
        {
            geneRecords.Add(geneName, new Dictionary<Guid, float>());
            geneHistoryAvarage.Add(geneName, new SortedDictionary<float, float>());
        }

        geneRecords[geneName][id] = value;

        geneHistoryAvarage[geneName].Add(Time.realtimeSinceStartup, calculateAvarage(geneName));

        OnGeneStatisticsUpdated?.Invoke(geneName);
    }

    public static void DelogGene(string geneName,Guid id)
    {
        geneRecords[geneName].Remove(id);
        geneHistoryAvarage[geneName].Add(Time.realtimeSinceStartup, calculateAvarage(geneName));
        
        OnGeneStatisticsUpdated?.Invoke(geneName);
    }

    public static void LogPopulation(string creatureName, bool born)
    { 
        if (!populationHistory.ContainsKey(creatureName) || !population.ContainsKey(creatureName))
        {
            populationHistory.Add(creatureName, new SortedDictionary<float, float>());
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
        
        OnPopulationUpdated?.Invoke();
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