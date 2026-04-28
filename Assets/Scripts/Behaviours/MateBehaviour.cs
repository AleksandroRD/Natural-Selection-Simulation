using UnityEngine;

class MateBehaviour : WanderBehavior
{
    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles muscles;
    private readonly Animal animal;
    private Vector3 currentDestination;
    private float wanderRadius = 5;

    private float matingTime = 3f;
    private float matingTimer = 0;

    public MateBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles, Animal animal) : base(muscles)
    {
        this.sensorySystem = sensorySystem;
        this.muscles = muscles;
        this.animal = animal;

        currentDestination = Simulation.PickRandomDectination(muscles.transform.position, wanderRadius);

    }

    public override void Perform()
    {
        Rabbit nearestMate = sensorySystem.LookFor<Rabbit>();
        
        if(nearestMate == null)
        {
            Wander();
        }

        if (nearestMate != null && nearestMate.IsReadyToMate() && nearestMate.Gender != animal.Gender)
        {
            currentDestination = nearestMate.transform.position;
        }

        if(nearestMate != null && muscles.HasArrived() && nearestMate.Gender != animal.Gender)
        {
            matingTimer += Time.deltaTime;

            if(matingTimer < matingTime) { return; }
            
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
                    animal.Replicate(nearestMate.Genome);
                }    
            }

            matingTimer = 0;
            
        }

        muscles.MoveTo(currentDestination);
    }

}