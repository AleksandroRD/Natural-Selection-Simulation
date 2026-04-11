using System.Collections.Generic;
using UnityEngine;

public class Rabbit : Animal
{
    public Genome Genome{get; protected set;}
    private SensoryNervousSystem sensorySystem;
    private Muscles muscles;

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
        this.Genome = genome;
        muscles = GetComponent<Muscles>();
        sensorySystem = GetComponent<SensoryNervousSystem>();
        CurrentEnergy = 100.0f;
        EnergyExpenditure = BaseExpenditure;

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
        
        muscles.SetMovementSpeed(genome.GetGeneValue("Speed Gene"));
        sensorySystem.SetSightRadius(genome.GetGeneValue("Sight Gene"));
        SetBehaviour( new WanderBehavior(muscles));
    }

    public override void Replicate()
    {
        MatingUrge = 0;
    }

    public override void Replicate(Genome otherGenome)
    {
        Genome childGenome = Genome.Recombine(otherGenome);

        GameObject childGameObject = new GameObject("Rabbit", typeof(Muscles), typeof(SensoryNervousSystem));

        childGameObject.AddComponent<Rabbit>().Initialize(childGenome);

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
}
