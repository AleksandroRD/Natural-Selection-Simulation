using System;
using UnityEngine;

class SearchForFoodBehaviour : WanderBehavior
{
    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles muscles;

    private Vector3 currentDestination;
    private float eatingTime = 0.5f;
    private float eatingTimer = 0;

    Action<float> eatingFunction;
    public SearchForFoodBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles, Action<float> eatingFunction) : base(muscles)
    {
        this.sensorySystem = sensorySystem;
        this.muscles = muscles;
        this.eatingFunction = eatingFunction;
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
            muscles.MoveTo(currentDestination);
        }

        //found food and arrived at foods location
        if (nearestFood != null && muscles.HasArrived())
        {
            nearestFood.StartConsumtion();
            eatingTimer += Time.deltaTime;

            if(eatingTimer >= eatingTime)
            {
                eatingFunction(nearestFood.FinishConsumption());

                eatingTimer = 0;
            }
        }
        
        if(nearestFood == null)
        {
            Wander();
        }
    }
}