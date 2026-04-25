using UnityEngine;

public class WanderBehavior : Behaviour
{
    private Muscles muscles;

    float wandertheta = 0;
    float wanderRadius = 1.5f; 
    float diversionStartDistance = 1;

    public WanderBehavior(Muscles muscles)
    {
        this.muscles = muscles;
        this.wandertheta = Random.Range(0,2*Mathf.PI);
    }

    ~WanderBehavior()
    {
        muscles.Stop();
    }

    public override void Perform()
    {
        wandertheta += Random.Range(-Mathf.PI / 15,Mathf.PI / 15);
        Vector3 wanderPoint = muscles.transform.position + muscles.transform.forward * 3f;
        Vector3 direction = default;
        Vector3 closestpoint = ClosestPointOnBoundsEdge(Simulation.GetSimulationBounds(), muscles.transform.position);
        
        float distanceToBorder = Vector3.Distance(closestpoint, muscles.transform.position);
        float diversionPower = (1 - distanceToBorder) * (diversionStartDistance / distanceToBorder);

        if(distanceToBorder < diversionStartDistance)
        {
            direction = (muscles.transform.position - closestpoint).normalized * diversionPower;
        }
        else
        {            
            float x = wanderRadius * Mathf.Cos(wandertheta);
            float z = wanderRadius * Mathf.Sin(wandertheta);
            wanderPoint += new Vector3(x,0,z);

            direction = (wanderPoint - muscles.transform.position).normalized;
        }
#if UNITY_EDITOR
        Debug.DrawLine(muscles.transform.position, muscles.transform.position + muscles.transform.forward * 3f, Color.yellow);
        // Draw line to wander point
        Debug.DrawLine(muscles.transform.position + muscles.transform.forward * 3f, wanderPoint, Color.red);
        // Draw the wander circle (approximate with line segments)
        DrawDebugCircle(muscles.transform.position + muscles.transform.forward * 3f, wanderRadius, Color.white);
        // Draw a cross at the wander point
        Debug.DrawRay(wanderPoint, Vector3.up * 0.3f, Color.red);        
#endif
        muscles.MoveInDirection(direction);
    }

public static Vector3 ClosestPointOnBoundsEdge(Bounds bounds, Vector3 point)
{
    float distLeft   = point.x - bounds.min.x;
    float distRight  = bounds.max.x - point.x;
    float distBottom = point.y - bounds.min.y;
    float distTop    = bounds.max.y - point.y;
    float distBack   = point.z - bounds.min.z;
    float distFront  = bounds.max.z - point.z;

    float minDist = Mathf.Min(distLeft, distRight, distBottom, distTop, distBack, distFront);

    if (minDist == distLeft)
    {
        return new Vector3(bounds.min.x, point.y, point.z);
    }
    else if (minDist == distRight)
    {
        return new Vector3(bounds.max.x, point.y, point.z);
    }
    else if (minDist == distBottom)
    {
        return new Vector3(point.x, bounds.min.y, point.z);
    }
    else if (minDist == distTop)
    {
        return new Vector3(point.x, bounds.max.y, point.z);
    }
    else if (minDist == distBack)
    {
        return new Vector3(point.x, point.y, bounds.min.z);
    }
    else
    {
        return new Vector3(point.x, point.y, bounds.max.z);
    }
}

#if UNITY_EDITOR
    void DrawDebugCircle(Vector3 center, float radius, Color color, int segments = 32)
    {
        float angleStep = 2 * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float a1 = angleStep * i;
            float a2 = angleStep * (i + 1);
            Vector3 p1 = center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius;
            Vector3 p2 = center + new Vector3(Mathf.Cos(a2), 0f, Mathf.Sin(a2)) * radius;
            Debug.DrawLine(p1, p2, color);
        }
    }
#endif
}