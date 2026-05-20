using System;
using UnityEngine;

class SearchForFoodBehaviour : SteeringBehaviour
{
    enum State
    {
        Searching,
        Found,
        GettingToFood,
        Eating
    }

    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles muscles;
    private readonly Timer eatingTimer;
    private readonly Action<float> eatingFunction;
    private const float eatingTime = 1;
    private Food nearestFood;
    State state = State.Searching;

    public SearchForFoodBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles, Action<float> eatingFunction) : base(muscles)
    {
        this.sensorySystem = sensorySystem;
        this.muscles = muscles;
        this.eatingFunction = eatingFunction;
        eatingTimer = new Timer(eatingTime); 

        eatingTimer.OnTimerFinished += () =>
        {
            if (nearestFood != null)
            {
                eatingFunction(nearestFood.FinishConsumption());
            }
            nearestFood = null;
            state = State.Searching;
        };
    }

    public override void Perform()
    {
        switch (state)
        {
            case State.Searching:
                nearestFood = sensorySystem.LookFor<Food>();

                //Wander();
                if(nearestFood != null && !nearestFood.isBeingConsumed) 
                {
                    state = State.Found;
                }
                break;
                
            case State.Found:
                //GoTo(nearestFood.transform.position);
            #if UNITY_EDITOR
                Debug.DrawLine(muscles.transform.position, nearestFood.transform.position, Color.red);
            #endif
                //if (HasArrived())
                {
                    nearestFood.StartConsumtion();
        
                    eatingTimer.Start();

                    state = State.Eating;
                }
                break;

            case State.Eating:
                eatingTimer.Tick();
                break;
        }
        
        base.Perform();
    }
}