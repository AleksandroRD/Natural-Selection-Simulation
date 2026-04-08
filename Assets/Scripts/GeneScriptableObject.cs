using UnityEngine;

[CreateAssetMenu(fileName = "GeneScriptableObject", menuName = "Scriptable Objects/GeneScriptableObject")]
public class GeneScriptableObject : ScriptableObject
{
    public string Name;

    public float MinValue;

    public float MaxValue;
    
    [Tooltip("Value in % chance")]
    [Range(0,100)]
    public float MutationChance;

    public float MaxMutationAmount;

    public float MaxCost;
}
