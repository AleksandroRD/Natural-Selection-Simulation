using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [SerializeField]
    private float timeScale = 1;
    [SerializeField]
    private Bounds simulationBounds;
    private static Bounds simulationBoundsInternal;
    [SerializeField]
    private GameObject rabitPrefab;
    [SerializeField]
    private List<GeneScriptableObject> initialRabbitGeneData;
    [SerializeField]
    private int startingNumberOfRabits;
    [SerializeField]
    private GameObject carrotPrefab;

    [SerializeField]
    private float startingNumberofCarrots;
    
    [SerializeField]
    private float timeToSpawnCarrot = 1;

    public static Bounds GetSimulationBounds()
    {
        return simulationBoundsInternal;
    }
    void Start()
    {
        Time.timeScale = timeScale;
        
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
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if(timer < timeToSpawnCarrot) {return; }
        
        var carrot = GameObject.Instantiate(carrotPrefab);
        carrot.transform.position = GetRandomPostion();
        
        timer = 0;
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
