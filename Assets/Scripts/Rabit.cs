using System.Collections.Generic;
using UnityEngine;

public class Rabbit : Animal
{
    public Genome Genome{get; protected set;}
    private SensoryNervousSystem sensorySystem;
    private Muscles muscles;
    public static int MaxID = 0;

    public bool IsReadyToMate()
    {
        return MatingUrge >= 100f;
    }

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
        SetBehaviour( new WanderBehavior(muscles));

        Statistics.LogPopulation("Rabbit", true);
    }

    public override void Replicate()
    {
        MatingUrge = 0;
    }

    public override void Replicate(Genome otherGenome)
    {
        Genome childGenome = Genome.Recombine(otherGenome);

        GameObject childGameObject = GameObject.Instantiate(this.gameObject);

        childGameObject.GetComponent<Rabbit>().Initialize(childGenome);

        MatingUrge = 0;
    }

    public override void Simulate()
    {
        base.Simulate();
    
        if (CurrentEnergy < 25 || (CurrentEnergy < 70 && !IsReadyToMate()))
        {
            SetBehaviour(new SearchForFoodBehaviour(sensorySystem, muscles, Eat));
        }
        else if (IsReadyToMate() && CurrentEnergy > 50)
        {
            SetBehaviour(new MateBehaviour(sensorySystem, muscles, this));
        }
        else if (CurrentEnergy > 70)
        {
            SetBehaviour(new WanderBehavior(muscles));
        }
    }

    protected override void Death()
    {
        Statistics.LogPopulation("Rabbit", false);
        base.Death();
    }
}
