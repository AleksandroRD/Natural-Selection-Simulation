using System.Collections.Generic;
using UnityEngine;

public class Rabbit : Animal
{
    public Genome genome{get; protected set;}
    private SensoryNervousSystem sensorySystem;
    private Muscles muscles;

    public void Initialize(List<GeneScriptableObject> initialGeneData)
    {
        Initialize(new Genome(initialGeneData));
    }

    public void Initialize(Genome genome)
    {
        this.genome = genome;
        muscles = GetComponent<Muscles>();
        sensorySystem = GetComponent<SensoryNervousSystem>();
        CurrentEnergy = 100.0f;
        EnergyExpenditure = BaseExpenditure;

        muscles.SetMovementSpeed(genome.GetGeneValue("Speed Gene"));
        sensorySystem.SetSightRadius(genome.GetGeneValue("Sight Gene"));
        SetBehaviour( new WanderBehavior(muscles));
    }

    public override void Replicate(Genome otherGenome)
    {
        Genome childGenome = genome.Recombine(otherGenome);

        GameObject childGameObject = new GameObject("Rabbit", typeof(Muscles), typeof(SensoryNervousSystem));

        childGameObject.AddComponent<Rabbit>().Initialize(childGenome);
    }
    bool IsReadyToMate()
    {
        return MatingUrge >= 100f;
    }

    public override void Simulate()
    {
        base.Simulate();

        if(CurrentEnergy < 25 || CurrentEnergy < 70)
        {
            SetBehaviour(new SearchForFoodBehaviour(sensorySystem, muscles, Eat));
        }

        //if(CurrentEnergy > 70)
        //{
        //    SetBehaviour( new WanderBehavior(muscles));
        //}
        // if (IsReadyToMate() && CurrentEnergy > 50)
        // {
        //     SetBehaviour(new MateBehaviour(sensorySystem, muscles));
        // }
    }
}
