using UnityEngine;

public abstract class ClassAttackBehaviour : ScriptableObject
{
    public abstract void Attack(Transform origin, Vector2 direction);
}
