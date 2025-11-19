using UnityEngine;

[CreateAssetMenu(fileName = "MageBasicAttack", menuName = "RPG/Attacks/MageBasic")]
public class MageBasicAttack : ClassAttackBehaviour
{
    [Header("Attack Mechanics")]
    public float fixedRange = 10f;
    public float maxHeight = 2f;
    public int pierceCount = 0;
    public ProjectileBase projectilePrefab;

    [Header("Curves")]
    public AnimationCurve trajectoryCurve;

    public override void Attack(Transform origin, Vector2 direction, PlayerStats stats, ProjectileFactory factory)
    {
        Vector3 targetPos = origin.position + (Vector3)direction * fixedRange;
        float speed = baseSpeed * stats.AttackSpeed.FinalValue;
        float duration = (speed > 0) ? fixedRange / speed : 0;
        float damage = baseDamage * stats.Damage.FinalValue;
        
        var arcStrategy = new BaseMovementStrategy(targetPos, duration, maxHeight, trajectoryCurve);

        factory.Spawn(
            prefab: projectilePrefab,
            position: origin.position,
            movementStrategy: arcStrategy,
            owner: ProjectileOwner.Player,
            damage: damage,
            pierceCount: pierceCount
        );
    }
}