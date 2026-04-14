using UnityEngine;

public class Gene
{
    public readonly string Name;
 
    public float Value{get; private set;}

    public readonly float MinValue;

    public readonly float MaxValue;
    
    public float MutationChance{get; private set;}

    public readonly float MaxMutationAmount;

    public readonly float MaxCost;
    public readonly float MinCost;
    public readonly float Cost;

    private Gene(string name, float minValue, float maxValue, float minCost, float maxCost, float mutationChance, float maxMutaionAmount, float value)
    {
        this.Name = name;
        this.Value = value;
        this.MinValue = minValue;
        this.MaxValue = maxValue;
        this.MutationChance = mutationChance;
        this.MaxMutationAmount = maxMutaionAmount;
        this.MinCost = minCost;
        this.MaxCost = maxCost;
        this.Cost = GetCost();
    }

    public Gene(GeneScriptableObject data)
    {
        this.Name = data.name;
        this.MinValue = data.MinValue;
        this.MaxValue = data.MaxValue;
        this.MinCost = data.MinCost;
        this.MaxCost = data.MaxCost;
        this.MutationChance = data.MutationChance;
        this.MaxMutationAmount = data.MaxMutationAmount;
        this.Value = Random.Range(MinValue,MaxValue);
        this.Cost = GetCost();
    }

    private float GetCost()
    {
        float t = (Value - MinValue) / (MaxValue - MinValue);
        t = Mathf.Clamp(t, 0f, 1f);
        return MinCost + t * (MaxCost - MinCost);
    }

    public Gene Replicate()
    {
        System.Random rnd = new System.Random();
        int result = rnd.Next(0,100);

        if (result < MutationChance) { return new Gene(Name, MinValue, MaxValue, MinCost, MaxCost, MutationChance, MaxMutationAmount ,Value); }

        float newValue;
        float mutationAmount = Random.Range(-MaxMutationAmount,MaxMutationAmount);

        if(Value + mutationAmount > MaxValue)
        {
            newValue = MaxValue;
        }
        else if(Value + mutationAmount < MinValue)
        {
            newValue = MinValue;
        }
        else
        {
            newValue = Value + mutationAmount;
        }

        return new Gene(Name, MinValue, MaxValue, MinCost, MaxCost, MutationChance, MaxMutationAmount ,newValue);
    }
}