using UnityEngine;

public class RoamingBehaviour : SteeringBehaviour
{
    public RoamingBehaviour(GameObject agent,float maxSpeed) : base(agent,maxSpeed)
    {
        
    }

    public override void Perform()
    {
        Wander();
        base.Perform();
    }
}