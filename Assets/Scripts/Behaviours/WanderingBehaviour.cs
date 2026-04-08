using UnityEngine;

public class WanderBehavior : Behaviour
{
    private Muscles muscles;

    private float wanderRadius = 10f;

    private Vector3 currentTarget;

    public WanderBehavior(Muscles muscles)
    {
        this.muscles = muscles;
        currentTarget = PickRandomDestination();
    }

    ~WanderBehavior()
    {
        muscles.Stop();
    }

    public override void Perform()
    {
        if (muscles.HasArrived())
        {  
            currentTarget = PickRandomDestination();
        }

        muscles.MoveTo(currentTarget);
    }

    private Vector3 PickRandomDestination()
    {
        //
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 offset = new Vector3(randomCircle.x, 0.2f, randomCircle.y);
        return muscles.transform.position + offset;
    }
}