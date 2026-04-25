using UnityEngine;

public enum MovementMode { None, Destination, Direction }

[RequireComponent(typeof(Rigidbody))]
public class Muscles : MonoBehaviour
{
    public float MovementSpeed {get; private set;}
    private readonly float targetPositionMargin = 0.15f;
    private readonly float rotationSpeed = 2f;
    private Rigidbody rb;
    public MovementMode CurrentMode { get; private set; } = MovementMode.None;
    private Vector3 currentDestination;
    private Vector3 currentDirection;
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
        CurrentMode = MovementMode.Destination;
        IsMoving = true;
    }

    public void MoveTo(Transform target)
    {
        MoveTo(target.position);
    }

    public void FleeFrom(Vector3 threat)
    {
        Vector3 direction = (transform.position - threat).normalized;
        MoveInDirectionIntern(direction);
    }

    public void MoveInDirection(Vector3 direction)
    {
        currentDirection = direction;
        CurrentMode = MovementMode.Direction;
        IsMoving = true;
    }

    void MoveInDirectionIntern(Vector3 direction)
    {
        direction.y = 0f;
        direction = direction.normalized;

        if (direction == Vector3.zero) return;
        Vector3 targetVelocity = direction * MovementSpeed;
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, MovementSpeed * 5f * Time.fixedDeltaTime);

        Quaternion targetRot = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime));
    }

    public bool HasArrived()
    {
        return Vector3.Distance(transform.position, currentDestination) <= targetPositionMargin;
    }

    public void Stop()
    {
        IsMoving = false;
        CurrentMode = MovementMode.None;
        rb.linearVelocity = Vector3.zero;
        currentDestination = transform.position; 
    }
    
    void FixedUpdate()
    {
        if (!IsMoving) return;
    
        if (CurrentMode == MovementMode.Destination)
        {
            if (HasArrived()) { Stop(); return; }
            MoveInDirectionIntern((currentDestination - transform.position).normalized);
        }
        else if (CurrentMode == MovementMode.Direction)
        {
            MoveInDirectionIntern(currentDirection);
        }
    }
}