using UnityEngine;

class MateBehaviour : WanderBehavior
{
    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles muscles;
    private readonly Animal animal;
    private readonly Timer timer;

    public bool isCurrentlyMating { get; protected set;} = false;
    private const float matingTime = 3f;

    Rabbit nearestMate;
    public MateBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles, Animal animal) : base(muscles)
    {
        this.sensorySystem = sensorySystem;
        this.muscles = muscles;
        this.animal = animal;

        timer = new Timer(matingTime);
        timer.OnTimerFinished += () => { 
            Mate(nearestMate); 
            nearestMate = null;
            isCurrentlyMating = false;
        };
    }

    public override void Perform()
    {        
        if (isCurrentlyMating)
        {
            timer.Tick();
            return;
        }

        if (nearestMate == null || !IsCompatibleMate(nearestMate))
        {
            nearestMate = sensorySystem.LookFor<Rabbit>();
            Wander();
            return;
        }

        muscles.SetDestination(nearestMate.transform.position);
#if UNITY_EDITOR
        Debug.DrawLine(muscles.transform.position, nearestMate.transform.position, Color.red);
#endif
        if(!muscles.HasArrived()){ return; }
        
        timer.Start();
        
        isCurrentlyMating = true;
    }

    private bool IsCompatibleMate(Animal candidate)
    {
        return candidate.Gender != animal.Gender && candidate.isSearchingMate();
    }

    private void Mate(Animal mate)
    {
        if(animal.Gender == Gender.Male)
        {
            animal.ReplicateMale();
        }
        else
        {
            animal.ReplicateFemale(mate.Genome);  
        }
    }
}