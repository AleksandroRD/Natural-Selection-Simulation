using UnityEngine;
public class SensoryNervousSystem : MonoBehaviour
{
    public float Radius{get; private set;}
    private Collider[] colliders;

    public void SetSightRadius(float radius)
    {
        this.Radius = radius;
    }

    public T LookFor<T>()
    {
        Transform transform = this.gameObject.transform;
        colliders = Physics.OverlapSphere(transform.position, Radius);
        float closestDistance = float.MaxValue;
        T closest = default;

        foreach(var collider in colliders){
            if(collider.gameObject == this.gameObject) { continue; }
            T newOne = collider.gameObject.GetComponent<T>();
            if(newOne == null) { continue; }

            float distance = Vector3.Distance(collider.transform.position, transform.position);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closest = newOne;
            }
        }

        return closest;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
#endif
}