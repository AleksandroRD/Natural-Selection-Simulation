using System;
using UnityEngine;

class SearchForFoodBehaviour : WanderBehavior
{
    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles muscles;
    private readonly Timer eatingTimer;
    private readonly Action<float> eatingFunction;
    private bool eating = false;
    private const float eatingTime = 1;
    private Food nearestFood;

    public SearchForFoodBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles, Action<float> eatingFunction) : base(muscles)
    {
        this.sensorySystem = sensorySystem;
        this.muscles = muscles;
        this.eatingFunction = eatingFunction;
        eatingTimer = new Timer(eatingTime); 

        eatingTimer.OnTimerFinished += () =>
        {
            eatingFunction(nearestFood.FinishConsumption());
            eating = false;
        };
    }

    public override void Perform()
    {
        if(eating)
        {
            eatingTimer.Tick();
            return;
        }

        if(nearestFood == null || nearestFood.isBeingConsumed && !eating) 
        {
            nearestFood = sensorySystem.LookFor<Food>(); 
            Wander();
            return; 
        }
        
        muscles.SetDestination(nearestFood.transform.position);
        
#if UNITY_EDITOR
        Debug.DrawLine(muscles.transform.position, nearestFood.transform.position, Color.red);
#endif
        if (!muscles.HasArrived()) { return; }

        nearestFood.StartConsumtion();
        
        eatingTimer.Start();

        eating = true;
    }
}