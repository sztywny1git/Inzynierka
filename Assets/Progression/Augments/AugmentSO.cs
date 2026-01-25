using UnityEngine;

[CreateAssetMenu(fileName = "New Augment", menuName = "Augment System/Augment")]
public class AugmentSO : ScriptableObject
{
    public string augmentName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public AugmentType augmentType;

    public float value;

    // Dla stackowalnych augmentoww
    public bool isStackable = false;
    public int maxStacks = 1;
}
