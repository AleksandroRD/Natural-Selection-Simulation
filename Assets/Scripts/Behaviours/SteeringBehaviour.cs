using UnityEngine;

public abstract class SteeringBehaviour : Behaviour
{
    private Muscles muscles;

    private float wandertheta = 0;

    private float stuckTimer = 0;
    private const float TARGET_MARGIN = 0.15f;
    private const float WANDER_RADIUS = 0.5f; 
    private const float MAX_FORCE = 1f;
    private const float MAX_SPEED = 2f;
    private const float ROTATION_SPEED = 2f;
    private const float ACCELERATION = 5f;
    private const float WISKER_LENGHT = 3f;
    private const float STUCK_SPEED_THRESHOLD = 1f;
    private const float STUCK_TIME_THRESHOLD = 0.6f;

    private Vector3 position {get => muscles.transform.position; set { muscles.transform.position = value;} }
    private Vector3 forward { get => muscles.transform.forward; }
    private Quaternion rotation { get => muscles.transform.rotation; set { muscles.transform.rotation = value;} }

    /// <summary>
    /// Sum of the all forces acting on the body
    /// </summary>
    private Vector3 steeringForce = Vector3.zero;
    private Vector3 steeringVelocity;
    public SteeringBehaviour(Muscles muscles)
    {
        this.muscles = muscles;
        this.wandertheta = 0;//Random.Range(-180,180);
    }

    public override void Perform()
    {
        AvoidObstacles();

        if(steeringForce.magnitude > MAX_FORCE)
        {
            steeringForce = steeringForce.normalized * MAX_FORCE;
        }

        steeringVelocity += steeringForce * ACCELERATION * Time.deltaTime;

        if(steeringVelocity.magnitude > MAX_SPEED)
        {
            steeringVelocity = steeringVelocity.normalized * MAX_SPEED;
        }

        //If the agent is barely moving but a force is being applied, it is likely stuck
        if (steeringVelocity.magnitude < STUCK_SPEED_THRESHOLD && steeringForce.magnitude > 0.1f)
        {
            Debug.Log("Stuck");
            stuckTimer += Time.deltaTime;

            if(stuckTimer >= STUCK_TIME_THRESHOLD)
            {
                rotation.SetLookRotation(-forward);
                stuckTimer = 0;
            }
        }

        position += steeringVelocity * Time.deltaTime;

        rotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(steeringVelocity.normalized), ROTATION_SPEED * Time.deltaTime);

        steeringForce = Vector3.zero;
    }

    public void Seek(Vector3 seekPoint)
    {
        Vector3 disiredVelocity = (seekPoint - position).normalized * MAX_SPEED;
        Vector3 seekForce = disiredVelocity - steeringVelocity;

        seekForce.y = 0;
        steeringForce += seekForce;
    }

    public void Flee(Vector3 fleePoint)
    {
        Seek(fleePoint * -1);
    }

    public void Wander()
    {
        //Change 3 to a constant
        Vector3 wanderPoint = position + muscles.transform.forward * 3f;
                
        wandertheta += Random.Range(-10.0f, 10.0f);
        wandertheta = NormalizeAngle(wandertheta);

        //making rotation relative to agent    
        Vector3 rotationVector = Quaternion.AngleAxis(wandertheta, Vector3.down) * muscles.transform.forward;
        wanderPoint += WANDER_RADIUS * rotationVector;

        Seek(wanderPoint);
    }
    
    private void AvoidObstacles()
    {
        (Vector3 direction, float length)[] whiskerDirections = new (Vector3, float)[]
        {
            (muscles.transform.forward,WISKER_LENGHT),
            (Quaternion.AngleAxis( 15f, Vector3.up) * muscles.transform.forward, WISKER_LENGHT * 0.5f),
            (Quaternion.AngleAxis(-15f, Vector3.up) * muscles.transform.forward, WISKER_LENGHT * 0.5f)
        };

        Vector3 avoidanceForce = Vector3.zero;
        foreach (var pair in whiskerDirections)
        {            
            Vector3 whiskerPosition = position + pair.direction * pair.length;

            if(!Physics.Raycast(position, pair.direction, out RaycastHit hit, pair.length, LayerMask.GetMask("Obstacle"))) 
            { 
                Debug.DrawLine(position, position + pair.direction * pair.length, Color.skyBlue);
                continue;
            }
            else
            {
                Debug.DrawLine(position, position + pair.direction * pair.length, Color.orange);
            }

            //distance from the center of the obstacle to the wisker position
            Vector3 toObstacle = (hit.collider.bounds.center - position).normalized;
            float lateral = Vector3.Dot(toObstacle, muscles.transform.right);

            Vector3 steerDir = -Mathf.Sign(lateral) * muscles.transform.right;

            float avoidancePower = 1 / (hit.distance / steeringVelocity.magnitude);
            avoidanceForce = steerDir * Mathf.Abs(avoidancePower);
        }

        avoidanceForce.y = 0;
        steeringForce += avoidanceForce;
    }

    float NormalizeAngle(float angle)
    {
        return ((angle + 180f) % 360f + 360f) % 360f - 180f;
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