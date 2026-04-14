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

    Action<float> eatingFunction;
    public SearchForFoodBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles, Action<float> eatingFunction)
    {
        this.sensorySystem = sensorySystem;
        this.muscles = muscles;
        this.eatingFunction = eatingFunction;

        currentDestination = Simulation.PickRandomDectination(muscles.transform.position, wanderRadius);
    }

    ~SearchForFoodBehaviour()
    {
        muscles.Stop();
    }

    public override void Perform()
    {
        Food nearestFood = sensorySystem.LookFor<Food>();

        if(nearestFood != null && !nearestFood.isBeingConsumed)
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
                eatingFunction(nearestFood.Consume());

                eatingTimer = 0;
            }
        }
        
        //if we creature stoped moving and see no food around
        if(nearestFood == null && muscles.HasArrived())
        {
            currentDestination = Simulation.PickRandomDectination(muscles.transform.position, wanderRadius);
        }

        muscles.MoveTo(currentDestination);
    }
}