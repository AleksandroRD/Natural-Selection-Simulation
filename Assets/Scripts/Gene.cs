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

    private Gene(string name, float minValue, float maxValue,float maxCost, float mutationChance, float maxMutaionAmount, float value)
    {
        this.Name = name;
        this.MinValue = minValue;
        this.MaxValue = maxValue;
        this.MaxCost = maxCost;
        this.MutationChance = mutationChance;
        this.Value = value;
        this.MaxMutationAmount = maxMutaionAmount;
    }

    public Gene(GeneScriptableObject data)
    {
        this.Name = data.name;
        this.MinValue = data.MinValue;
        this.MaxValue = data.MaxValue;
        this.MaxCost = data.MaxCost;
        this.MutationChance = data.MutationChance;
        this.MaxMutationAmount = data.MaxMutationAmount;
        this.Value = Random.Range(MinValue,MaxValue);
    }
    
    public Gene Replicate()
    {
        System.Random rnd = new System.Random();
        int result = rnd.Next(0,100);

        if (result < MutationChance) { return new Gene(Name, MinValue, MaxValue, MaxCost, MutationChance, MaxMutationAmount ,Value); }

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

        return new Gene(Name, MinValue, MaxValue, MaxCost, MutationChance, MaxMutationAmount ,newValue);
    }
}