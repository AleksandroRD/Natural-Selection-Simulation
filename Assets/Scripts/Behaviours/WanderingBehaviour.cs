using UnityEngine;

public abstract class WanderBehavior : Behaviour
{
    private Muscles muscles;

    private float wandertheta = 0;
    private float wanderRadius = 1.5f; 
    private float diversionStartDistance = 1.5f;

    public WanderBehavior(Muscles muscles)
    {
        this.muscles = muscles;
        this.wandertheta = Random.Range(-180,180);
    }

    ~WanderBehavior()
    {
        muscles.Stop();
    }

    protected void Wander()
    {
        Vector3 wanderPoint = muscles.transform.position + muscles.transform.forward * 3f;
        Vector3 direction = default;
        Vector3 closestpoint = ClosestPointOnBoundsEdge(Simulation.GetSimulationBounds(), muscles.transform.position);
        
        float distanceToBorder = Vector3.Distance(closestpoint, muscles.transform.position);
        float diversionPower = 1 + (1 - distanceToBorder) * (diversionStartDistance / distanceToBorder);

        if(distanceToBorder < diversionStartDistance)
        {
            //angle to apply maximum turn to avoid simulation border
            wandertheta = Mathf.Sign(wandertheta) * 90.0f;
            //making rotation relative to agent
            Vector3 rotationVector = Quaternion.AngleAxis(wandertheta, Vector3.down) * muscles.transform.forward;
            wanderPoint += wanderRadius * rotationVector * diversionPower;
        }
        else
        { 
            wandertheta += Random.Range(-12.0f, 12.0f);

            //making rotation relative to agent           
            Vector3 rotationVector = Quaternion.AngleAxis(wandertheta, Vector3.down) * muscles.transform.forward;
            wanderPoint += wanderRadius * rotationVector;
        }

        direction = (wanderPoint - muscles.transform.position).normalized;
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
    float distBack   = point.z - bounds.min.z;
    float distFront  = bounds.max.z - point.z;

    float minDist = Mathf.Min(distLeft, distRight, distBack, distFront);

    if (minDist == distLeft)
    {
        return new Vector3(bounds.min.x, point.y, point.z);
    }
    else if (minDist == distRight)
    {
        return new Vector3(bounds.max.x, point.y, point.z);
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