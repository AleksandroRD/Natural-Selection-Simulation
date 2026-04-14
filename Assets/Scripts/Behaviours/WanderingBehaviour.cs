using UnityEngine;

public class WanderBehavior : Behaviour
{
    private Muscles muscles;

    private float wanderRadius = 10f;

    private Vector3 currentTarget;

    public WanderBehavior(Muscles muscles)
    {
        this.muscles = muscles;
        currentTarget = Simulation.PickRandomDectination(muscles.transform.position, wanderRadius);
    }

    ~WanderBehavior()
    {
        muscles.Stop();
    }

    public override void Perform()
    {
        if (muscles.HasArrived())
        {  
            currentTarget = Simulation.PickRandomDectination(muscles.transform.position, wanderRadius);
        }

        muscles.MoveTo(currentTarget);
    }

}