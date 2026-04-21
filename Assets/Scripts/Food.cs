using UnityEngine;

public class Food : MonoBehaviour
{
    public float nutritionalValue { get; protected set; }

    [SerializeField]
    private float minValue;
    [SerializeField]
    private float maxValue;

    public bool isBeingConsumed{get; private set;} = false;
    public void StartConsumtion()
    {
        isBeingConsumed = true;
    }

    public float FinishConsumption()
    {   
        Destroy(gameObject);
        return nutritionalValue;
    }

    void Awake()
    {
        nutritionalValue = Random.Range(minValue, maxValue);
        float scale = Mathf.Lerp(0,1, nutritionalValue/maxValue);
        this.gameObject.transform.localScale = Vector3.Scale(new Vector3(scale,scale,scale), this.gameObject.transform.localScale);
    }
}