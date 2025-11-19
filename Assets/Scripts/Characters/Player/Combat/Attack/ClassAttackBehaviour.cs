using UnityEngine;

// Base class for all attack types
public abstract class ClassAttackBehaviour : ScriptableObject
{
    public float baseDamage = 1f;
    public float baseSpeed = 10f;

    public enum AttackType { Melee, Projectile, AoE }
    public AttackType attackType;

    // Main attack method, optional projectile factory parameter
    public abstract void Attack(Transform origin, Vector2 direction, PlayerStats stats, ProjectileFactory factory = null);
}