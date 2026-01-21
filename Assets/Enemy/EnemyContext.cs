using UnityEngine;

public sealed class EnemyContext
{
    public EnemyBrain Brain { get; }
    public Transform Transform { get; }

    public EnemyMovement Movement { get; }
    public EnemyTargetProvider TargetProvider { get; }
    public EnemyMeleeAttack MeleeAttack { get; }

    public EnemyContext(
        EnemyBrain brain,
        EnemyMovement movement,
        EnemyTargetProvider targetProvider,
        EnemyMeleeAttack meleeAttack)
    {
        Brain = brain;
        Transform = brain.transform;

        Movement = movement;
        TargetProvider = targetProvider;
        MeleeAttack = meleeAttack;
    }
}
