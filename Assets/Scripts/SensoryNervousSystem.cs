using UnityEngine;
public class SensoryNervousSystem : MonoBehaviour
{
    public float Radius{get; private set;}
    private Collider[] colliders = new Collider[64];

    public void SetSightRadius(float radius)
    {
        this.Radius = radius;
    }

    public T LookFor<T>()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, Radius,colliders);
        
        float closestDistance = float.MaxValue;
        T closest = default;
        T newOne;

        for(int i = 0; i < count; i++ ){
            if(colliders[i].gameObject == this.gameObject) { continue; }
            
            if(!colliders[i].gameObject.TryGetComponent<T>(out newOne)) { continue; }

            float distance = Vector3.Distance(colliders[i].transform.position, transform.position);
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
        DrawDebugCircle(transform.position,Radius,new Color(0f, 1f, 0.4f, 0.9f));
    }

    void DrawDebugCircle(Vector3 center, float radius, Color color, int segments = 32)
    {
        float angleStep = 2 * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float a1 = angleStep * i;
            float a2 = angleStep * (i + 1);
            Vector3 p1 = center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius;
            Vector3 p2 = center + new Vector3(Mathf.Cos(a2), 0f, Mathf.Sin(a2)) * radius;
            Gizmos.color = color;
            Gizmos.DrawLine(p1, p2);
        }
    }
#endif
}