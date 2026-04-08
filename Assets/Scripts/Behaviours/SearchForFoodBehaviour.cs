using System;
using UnityEngine;

class SearchForFoodBehaviour : Behaviour
{
    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles muscles;

    private Vector3 currentDestination;
    private float eatingTime = 0.5f;
    private float eatingTimer = 0;

    private float wanderRadius = 5;

    Action<int> eatingFunction;
    public SearchForFoodBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles, Action<int> eatingFunction)
    {
        this.sensorySystem = sensorySystem;
        this.muscles = muscles;
        this.eatingFunction = eatingFunction;

        currentDestination = PickRandomDestination();
    }

    ~SearchForFoodBehaviour()
    {
        muscles.Stop();
    }

    public override void Perform()
    {
        Food nearestFood = sensorySystem.LookFor<Food>();

        if(nearestFood != null)
        {
            currentDestination = nearestFood.transform.position;
        }

        //found food and arrived at foods location
        if (nearestFood != null && muscles.HasArrived())
        {
            eatingTimer += Time.deltaTime;

            if(eatingTimer >= eatingTime)
            {
                GameObject.Destroy(nearestFood.gameObject);
                eatingFunction(50);

                eatingTimer = 0;
            }
        }
        
        //if we creature stoped moving and see no food around
        if(nearestFood == null && muscles.HasArrived())
        {
            currentDestination = PickRandomDestination();
        }

        muscles.MoveTo(currentDestination);
    }

    private Vector3 PickRandomDestination()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * wanderRadius;
        Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y);
        return muscles.transform.position + offset;
        
    }
}