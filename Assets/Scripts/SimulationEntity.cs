using UnityEngine;

public abstract class SimulationEntity : MonoBehaviour
{
    public abstract void Simulate();

    public void FixedUpdate()
    {
        Simulate();
    }
}
