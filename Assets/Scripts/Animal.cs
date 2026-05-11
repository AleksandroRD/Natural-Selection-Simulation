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
    public bool IsMating { get; private set; }
    
    public Behaviour CurrentBehaviour { get; protected set;}
    public Genome Genome { get; protected set; }
    
    public virtual bool IsReadyToMate()
    {
        return MatingUrge >= 100f && !IsMating;
    }

    public void StartMating()
    {
        IsMating = true;
    }

    public void FinishMating()
    {
        IsMating = false;
    }

    public abstract void Replicate(Genome otherGenome);
    public abstract void Replicate();

    public override void Simulate()
    {
        MatingUrge += SexDrive;
        CurrentEnergy -= EnergyExpenditure;

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
        GameObject.Destroy(this.gameObject);
    }
}