using System;
using UnityEngine;

public enum Gender
{
    Male,
    Female
}

public abstract class Animal : SimulationEntity
{
    public float CurrentEnergy { get; protected set; }
    public float EnergyExpenditure { get; protected set; } = 0;
    public const float MAXENERGY = 100f;

    public float MatingUrge { get; protected set; } = 0;
    public float SexDrive { get; protected set; }

    public Gender Gender { get; protected set; }
    
    public Behaviour CurrentBehaviour { get; protected set;}
    public Genome Genome { get; protected set; }
    
    public virtual bool IsReadyToMate()
    {
        return MatingUrge >= 100f;
    }

    public Behaviour getCurrentBehaviour()
    {
        return CurrentBehaviour;
    }

    public T getCurrentBehaviour<T>() where T : Behaviour
    {
        return CurrentBehaviour as T ?? throw new InvalidCastException($"CurrentBehaviour is not of type {typeof(T)}");
    }

    public abstract void ReplicateFemale(Genome otherGenome);
    public abstract void ReplicateMale();

    public override void Simulate()
    {
        MatingUrge += SexDrive;
        CurrentEnergy -= EnergyExpenditure / 50f; // 50 updates per second for FixedUpdate()

        if(CurrentEnergy <= 0)
        {
            Death();

            return;
        }

        CurrentBehaviour.Perform();
    }

    protected void SetBehaviour(Behaviour newBehaviour)
    {
        if(CurrentBehaviour?.GetType() == newBehaviour.GetType()) { return; }
        CurrentBehaviour = newBehaviour;
    }

    protected void Eat(float amount)
    {
        if(CurrentEnergy + amount < MAXENERGY)
        {
            CurrentEnergy += amount;
        }
        else
        {
            CurrentEnergy = MAXENERGY;
        }
    }

    protected virtual void Death()
    {
        Genome.Dispose();
        GameObject.Destroy(this.gameObject);
    }
}