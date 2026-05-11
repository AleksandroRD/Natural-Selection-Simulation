using UnityEngine;

class MateBehaviour : WanderBehavior
{
    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles muscles;
    private readonly Animal animal;
    private readonly Timer timer;

    private bool isCurrentlyMating = false;
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
        nearestMate = sensorySystem.LookFor<Rabbit>();
        
        if (nearestMate == null || !IsCompatibleMate(nearestMate))
        {
            Wander();
            return;
        }

        muscles.MoveTo(nearestMate.transform.position);

        if (!muscles.HasArrived()){ return; }
        
        if(isCurrentlyMating == false)
        {
            timer.Start();

            isCurrentlyMating = true;
        }
#if UNITY_EDITOR
    Debug.DrawLine(muscles.transform.position, nearestMate.transform.position, Color.red);
#endif
        timer.Tick();
    }

    private bool IsCompatibleMate(Animal candidate)
    {
        return candidate.Gender != animal.Gender && candidate.IsReadyToMate();
    }

    private void Mate(Animal mate)
    {
        if(animal.Gender == Gender.Male)
        {
            animal.Replicate();
        }

        if(animal.Gender == Gender.Female)
        {
            System.Random rnd = new System.Random();
            int numberOfChildren = rnd.Next(0,4);

            for(int i = 0; i < numberOfChildren; i++)
            {    
                animal.Replicate(mate.Genome);
            }    
        }
    }
}