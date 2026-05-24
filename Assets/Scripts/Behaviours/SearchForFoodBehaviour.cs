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
    private readonly Timer eatingTimer;
    private readonly Action<float> eatingFunction;
    private const float eatingTime = 1;
    private Food nearestFood;
    State state = State.Searching;

    public SearchForFoodBehaviour(SensoryNervousSystem sensorySystem, GameObject agent, Action<float> eatingFunction, float maxSpeed) : base(agent,maxSpeed)
    {
        this.sensorySystem = sensorySystem;
        this.agent = agent;
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

                Wander();
                if(nearestFood != null && !nearestFood.isBeingConsumed) 
                {
                    state = State.Found;
                }
                break;
                
            case State.Found:
                if(nearestFood == null && nearestFood.isBeingConsumed) { state = State.Searching; return; }

                Seek(nearestFood.transform.position);
            #if UNITY_EDITOR
                Debug.DrawLine(position, nearestFood.transform.position, Color.red);
            #endif
                if (HasArrived(nearestFood.transform.position))
                {
                    Stop();

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