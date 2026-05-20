using UnityEngine;

public class RoamingBehaviour : SteeringBehaviour
{
    public RoamingBehaviour(Muscles muscles) : base(muscles)
    {
        
    }

    public override void Perform()
    {
        Wander();
        base.Perform();
    }
}