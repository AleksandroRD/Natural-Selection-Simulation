using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Muscles : MonoBehaviour
{
    public float MovementSpeed {get; set;}
    private const float TARGET_MARGIN = 0.15f;
    private const float ROTATION_SPEED = 2f;
    public Rigidbody rb;
    private Vector3 currentDestination = Vector3.positiveInfinity;
    private Vector3 currentDirection = Vector3.zero;
    public bool IsMoving {get; private set;} = false;
    
    public Vector3 velocity 
    {
        get => rb.linearVelocity;
        set
        {
            rb.linearVelocity = value;
        }
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetDestination(Vector3 destination)
    {
        if(destination == currentDestination) { return; }
        currentDestination = destination;
        currentDirection = (currentDestination - transform.position).normalized;
        IsMoving = true;
    }

    public void SetDestination(Transform target)
    {
        SetDestination(target.position);
    }

    public void MoveInDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) { return; }
        currentDestination = Vector3.positiveInfinity; 
        currentDirection = direction;
        IsMoving = true;
    }

    void MoveInDirectionIntern(Vector3 direction)
    {
        direction.y = 0f;
        direction = direction.normalized;

        if (direction == Vector3.zero) { return; }
        Vector3 targetVelocity = direction * MovementSpeed;
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, MovementSpeed);

        Quaternion targetRot = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, ROTATION_SPEED * Time.deltaTime));
    }

    public bool HasArrived()
    {
        return Vector3.Distance(transform.position, currentDestination) <= TARGET_MARGIN;
    }

    public void Stop()
    {
        IsMoving = false;
        rb.linearVelocity = Vector3.zero;
        currentDirection = Vector3.zero;
    }
    
    void FixedUpdate()
    {
        // if(!IsMoving){ return; }
        
        // if(HasArrived() == true)
        // {
        //     Stop();
        //     return;
        // }

        // MoveInDirectionIntern(currentDirection);
    }
}