using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [SerializeField]
    private int startingNumberOfRabits;
    [SerializeField]
    private GameObject rabitPrefab;
    [SerializeField]
    private List<GeneScriptableObject> initialRabbitGeneData;
    [SerializeField]
    private Bounds simulationBounds;
    private static Bounds simulationBoundsInternal;
    [SerializeField]
    private GameObject carrotPrefab;

    [SerializeField]
    private float startingNumberofCarrots;
    
    void Start()
    {
        simulationBoundsInternal = simulationBounds;
        for(int i = 0; i < startingNumberOfRabits;i++)
        {
            GameObject rabit = GameObject.Instantiate(rabitPrefab);
            rabit.transform.position = GetRandomPostion();

            rabit.GetComponent<Rabbit>().Initialize(initialRabbitGeneData);

        }

        for(int i = 0; i < startingNumberofCarrots; i++)
        {
            GameObject carrot = GameObject.Instantiate(carrotPrefab);
            carrot.transform.position = GetRandomPostion();
        }
    }

    private float timer = 0;
    [SerializeField]
    private float timeToSpawnCarrot = 1;
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if(timer < timeToSpawnCarrot) {return; }
        
        var carrot = GameObject.Instantiate(carrotPrefab);
        carrot.transform.position = GetRandomPostion();
        
        timer = 0;
    }

    public static Vector3 PickRandomDectination(Vector3 origin, float radius)
    {
        Vector3 offset;
        
        do
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            offset = new Vector3(randomCircle.x, 0.1f, randomCircle.y);
        }
        while(!simulationBoundsInternal.Contains(origin + offset));
        
        return origin + offset;
    }

    public static Vector3 GetRandomPostion()
    {
        return new Vector3(
            Random.Range(simulationBoundsInternal.min.x, simulationBoundsInternal.max.x),
            0.3f,
            Random.Range(simulationBoundsInternal.min.z, simulationBoundsInternal.max.z)   
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,0,0);
        Gizmos.DrawWireCube(transform.position,simulationBounds.size);
    }
#endif
}
