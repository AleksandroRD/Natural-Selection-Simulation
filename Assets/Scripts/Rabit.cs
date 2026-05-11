using System;
using System.Collections.Generic;
using UnityEngine;

public class Rabbit : Animal
{
    private SensoryNervousSystem sensorySystem;
    private Muscles muscles;
    public static int MaxID = 0;

    public void Initialize(List<GeneScriptableObject> initialGeneData)
    {
        Initialize(new Genome(initialGeneData));
    }

    public void Initialize(Genome genome)
    {
        this.name = "Rabbit " + MaxID++;
        this.Genome = genome;

        muscles = GetComponent<Muscles>();
        sensorySystem = GetComponent<SensoryNervousSystem>();
        CurrentEnergy = 100.0f;
        
        foreach(var gene in Genome.genes)
        {
            EnergyExpenditure += gene.Value.Cost;
        }
        
        System.Random rnd = new System.Random();
        int result = rnd.Next(0,2);
        if (result == 0)
        {
            Gender = Gender.Male;
        }
        else
        {
            Gender = Gender.Female;
        }
        
        SexDrive = genome.GetGeneValue("Sex Drive");
        muscles.SetMovementSpeed(genome.GetGeneValue("Speed Gene"));
        sensorySystem.SetSightRadius(genome.GetGeneValue("Sight Gene"));
        SetBehaviour( new RoamingBehaviour(muscles));

        Statistics.LogPopulation("Rabbit", true);
    }

    public override void Replicate()
    {
        MatingUrge = 0;
    }

    public override void Replicate(Genome otherGenome)
    {
        Genome childGenome = Genome.Recombine(otherGenome);

        Vector2 newPosition = UnityEngine.Random.onUnitCircle;
        Vector2 newRotation = UnityEngine.Random.onUnitCircle;
        GameObject childGameObject = GameObject.Instantiate(this.gameObject, this.transform.position + new Vector3(newPosition.x,0,newPosition.y), Quaternion.LookRotation(newRotation));

        childGameObject.GetComponent<Rabbit>().Initialize(childGenome);

        MatingUrge = 0;
    }

    public override void Simulate()
    {
        base.Simulate();
    
        Type desiredBehaviour = GetDesiredBehaviourType();
        
        if (CurrentBehaviour?.GetType() == desiredBehaviour) { return; }
        
        if (desiredBehaviour == typeof(SearchForFoodBehaviour))
        {
            SetBehaviour(new SearchForFoodBehaviour(sensorySystem, muscles, Eat));
        }
        else if (desiredBehaviour == typeof(MateBehaviour))
        { 
            SetBehaviour(new MateBehaviour(sensorySystem, muscles, this));
        } 
    }
    
    private Type GetDesiredBehaviourType()
    {
        if (CurrentEnergy > 70 && IsReadyToMate())
        {
            return typeof(MateBehaviour); 
        }
        else 
        {
            return typeof(SearchForFoodBehaviour); 
        }
    }

    protected override void Death()
    {
        Statistics.LogPopulation("Rabbit", false);
        base.Death();
    }
}
