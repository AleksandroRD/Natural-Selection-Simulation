using UnityEngine;

public abstract class Animal : SimulationEntity
{
    public float CurrentEnergy{get; protected set;}

    [field: SerializeField]
    public float BaseExpenditure{get; protected set;}
    public float EnergyExpenditure{get; protected set;}
    public float MatingUrge{get; protected set;}
    public Behaviour CurrentBehaviour{get; protected set;}
    public const float MAXENERGY = 100f;
    public abstract void Replicate(Genome otherGenome);

    public override void Simulate()
    {
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

    protected void Eat(int amount)
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
    public void Death()
    {
        GameObject.Destroy(this.gameObject);
    }


}