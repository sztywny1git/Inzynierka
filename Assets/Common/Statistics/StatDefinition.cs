using UnityEngine;

[CreateAssetMenu(fileName = "New Stat Definition", menuName = "Stats/Stat Definition")]
public class StatDefinition : ScriptableObject
{
    [Header("Properties")]
    public bool IsInteger = false;
    public bool HasMaximum = false;
    public float MaxValue = float.MaxValue;
}