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
    [SerializeField]
    private GameObject carrotPrefab;
    void Start()
    {
        for(int i = 0; i < startingNumberOfRabits;i++)
        {
            GameObject rabit = GameObject.Instantiate(rabitPrefab);
            rabit.name = "Rabbit " + GUID.Generate();
            rabit.GetComponent<Rabbit>().Initialize(initialRabbitGeneData);

            rabit.transform.position = new Vector3(
                Random.Range(simulationBounds.min.x,simulationBounds.max.x),
                0.3f,
                Random.Range(simulationBounds.min.z,simulationBounds.max.z)   
            );
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
        
        carrot.transform.position = new Vector3(
            Random.Range(simulationBounds.min.x,simulationBounds.max.x),
            0.3f,
            Random.Range(simulationBounds.min.z,simulationBounds.max.z)   
        );
        
        timer = 0;
    }
}
