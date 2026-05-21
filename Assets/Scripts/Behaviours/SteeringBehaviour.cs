using System.Linq;
using UnityEngine;

public abstract class SteeringBehaviour : Behaviour
{
    private GameObject agent;

    private float wandertheta = 0;

    private const float TARGET_MARGIN = 0.15f;
    private const float WANDER_RADIUS = 0.5f; 
    private const float MAX_FORCE = 1f;
    private readonly float MAX_SPEED = 2f;
    private const float ROTATION_SPEED = 2f;
    private const float ACCELERATION = 5f;
    private const float WISKER_LENGHT = 3f;

    protected Vector3 position {get => agent.transform.position; set { agent.transform.position = value;} }
    protected Vector3 forward { get => agent.transform.forward; }
    protected Quaternion rotation { get => agent.transform.rotation; set { agent.transform.rotation = value;} }

    /// <summary>
    /// Sum of the all forces acting on the body
    /// </summary>
    private Vector3 steeringForce = Vector3.zero;
    private Vector3 steeringVelocity;
    private Vector3? seekTarget;
    public SteeringBehaviour(GameObject agent, float maxSpeed)
    {
        this.agent = agent;
        this.wandertheta = Random.Range(-180,180);
        this.MAX_SPEED = maxSpeed;
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
        Vector3 wanderPoint = position + forward * 3f;
                
        wandertheta += Random.Range(-10.0f, 10.0f);
        wandertheta = NormalizeAngle(wandertheta);

        //making rotation relative to agent    
        Vector3 rotationVector = Quaternion.AngleAxis(wandertheta, Vector3.down) * forward;
        wanderPoint += WANDER_RADIUS * rotationVector;

        Seek(wanderPoint);
    }

    private void AvoidObstacles()
    {
        (Vector3 direction, float length)[] whiskerDirections = new (Vector3, float)[]
        {
            (forward, WISKER_LENGHT * 1.5f),  // Central ray - longest, highest priority
            (Quaternion.AngleAxis(20f, Vector3.up) * forward, WISKER_LENGHT * 0.8f),
            (Quaternion.AngleAxis(-20f, Vector3.up) * forward, WISKER_LENGHT * 0.8f),
            (Quaternion.AngleAxis(45f, Vector3.up) * forward, WISKER_LENGHT * 0.5f),
            (Quaternion.AngleAxis(-45f, Vector3.up) * forward, WISKER_LENGHT * 0.5f)
        };

        Vector3 avoidanceForce = Vector3.zero;
        bool centralRayHit = false;
        int totalHits = 0;
        Vector3 closestObstacleNormal = Vector3.zero;
        float closestDistance = float.MaxValue;

        // First pass: check central ray (highest priority)
        var centralRay = whiskerDirections[0];
        if (Physics.Raycast(position, centralRay.direction, out RaycastHit centralHit, centralRay.length, LayerMask.GetMask("Obstacle")))
        {
            centralRayHit = true;
            totalHits++;
            closestObstacleNormal = Vector3.ProjectOnPlane(centralHit.normal, Vector3.up).normalized;
            closestDistance = centralHit.distance;
            Debug.DrawLine(position, position + centralRay.direction * centralRay.length, Color.orange);
        }
        else
        {
            Debug.DrawLine(position, position + centralRay.direction * centralRay.length, Color.skyBlue);
        }

        // Second pass: check whiskers only if central ray didn't hit
        if (!centralRayHit)
        {
            for (int i = 1; i < whiskerDirections.Length; i++)
            {
                var (direction, length) = whiskerDirections[i];

                if (!Physics.Raycast(position, direction, out RaycastHit hit, length, LayerMask.GetMask("Obstacle")))
                {
                    Debug.DrawLine(position, position + direction * length, Color.skyBlue);
                    continue;
                }

                totalHits++;

                // Track the closest obstacle
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestObstacleNormal = Vector3.ProjectOnPlane(hit.normal, Vector3.up).normalized;
                }

                Debug.DrawLine(position, position + direction * length, Color.orange);
            }
        }

        if (totalHits == 0) { return; }

        // Calculate steering direction - prefer to steer along the wall rather than directly away
        Vector3 steerDir = Vector3.Cross(closestObstacleNormal, Vector3.up).normalized;

        // In corners (multiple hits), bias steering toward the agent's current velocity direction
        // This prevents oscillation between walls
        if (totalHits >= 2)
        {
            // Blend between wall-parallel steering and velocity direction based on angle
            float velocityAlignment = Vector3.Dot(steeringVelocity.normalized, forward);
            steerDir = Vector3.Lerp(steerDir, steeringVelocity.normalized, velocityAlignment * 0.5f);
        }

        float avoidancePower = 1f / (closestDistance / steeringVelocity.magnitude + 0.01f);
        avoidanceForce = steerDir * avoidancePower;

        // Clamp the avoidance force to prevent extreme turns away from target
        float maxAvoidanceAngle = 60f;
        float angleBetween = Vector3.Angle(avoidanceForce, forward);
        if (angleBetween > maxAvoidanceAngle)
        {
            avoidanceForce = avoidanceForce.normalized * (Mathf.Cos(maxAvoidanceAngle * Mathf.Deg2Rad) * avoidanceForce.magnitude);
        }
        
        avoidanceForce.y = 0;
        steeringForce += avoidanceForce;
    }

    public bool HasArrived(Vector3 target)
    {
        Vector3 diff = target - position;
        diff.y = 0;
        return diff.sqrMagnitude <= TARGET_MARGIN * TARGET_MARGIN;
    }

    float NormalizeAngle(float angle)
    {
        return ((angle + 180f) % 360f + 360f) % 360f - 180f;
    }
}