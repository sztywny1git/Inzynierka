using UnityEngine;

[CreateAssetMenu(fileName = "NewClass", menuName = "RPG/Class")]
public class ClassData : ScriptableObject
{
    public string className;

    [Header("Sprite")]
    public Sprite classSprite;

    [Header("Multipliers")]
    public float healthMultiplier = 1f;
    public float attackSpeedMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;
    public float damageMultiplier = 1f;

    [Header("Attack Behaviour")]
    public ClassAttackBehaviour attackBehaviour;
}