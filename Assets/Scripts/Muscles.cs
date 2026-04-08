using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Muscles : MonoBehaviour
{
    public float MovementSpeed {get; private set;}
    private readonly float targetPositionMargin = 0.1f;
    private Rigidbody rb;
    private Vector3 currentDestination;
    public bool IsMoving {get; private set;} = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetMovementSpeed(float movementSpeed)
    {
        MovementSpeed = movementSpeed;
    }

    public void MoveTo(Vector3 destination)
    {
        currentDestination = destination;
        IsMoving = true;
    }

    public void MoveTo(Transform target)
    {
        MoveTo(target.position);
    }
    
    public bool HasArrived()
    {
        return Vector3.Distance(transform.position, currentDestination) <= targetPositionMargin;
    }

    public void Stop()
    {
        IsMoving = false;
        rb.linearVelocity = Vector3.zero;
        currentDestination = transform.position;
    }

    void FixedUpdate()
    {
        if (!IsMoving) return;

        if (HasArrived())
        {
            Stop();
            return;
        }

        Vector3 direction = (currentDestination - transform.position).normalized;
        rb.linearVelocity = direction * MovementSpeed;
    }
}