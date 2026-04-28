using UnityEngine;

public class RoamingBehaviour : WanderBehavior
{
    public RoamingBehaviour(Muscles muscles) : base(muscles)
    {
        
    }

    public override void Perform()
    {
        Wander();
    }
}