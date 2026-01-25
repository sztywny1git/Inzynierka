using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee Ability")]
public class MeleeAbility : DamageAbility, IStepAbility
{
    [Header("Runner Configuration")]
    [SerializeField] private AbilityRunner _runnerPrefab;

    [Header("Melee Configuration")]
    [SerializeField] private AbilityHitbox _hitboxPrefab;
    [SerializeField] private float _spawnDelay = 0.1f;
    [SerializeField] private float _spreadAngle = 45f;
    [SerializeField] private float _positionSpread = 0.5f;

    public override void Execute(AbilityContext context, AbilitySnapshot snapshot)
    {
        if (_runnerPrefab != null)
        {
            var runner = context.Spawner.Spawn(_runnerPrefab, context.Origin.position, Quaternion.identity);
            runner.Initialize(this, context, snapshot, AttackCount, _spawnDelay, null);
        }
    }

    public void OnRunnerStep(AbilityContext context, AbilitySnapshot snapshot, int index, AbilityRunner runner)
    {
        Vector3 baseAimDir = (context.AimLocation - context.Origin.position).normalized;
        baseAimDir.z = 0;

        int totalCount = AttackCount;
        float angleStep = (totalCount > 1) ? _spreadAngle / (totalCount - 1) : 0f;

        Vector3 targetScale = Vector3.one;
        if (baseAimDir.x < 0) targetScale.y = -1f;

        Vector3 perpendicularDir = new Vector3(-baseAimDir.y, baseAimDir.x, 0);

        float multiplier = 0f;
        if (index > 0)
        {
            multiplier = (index % 2 != 0) ? ((index + 1) / 2f) : -((index) / 2f);
        }

        float currentAngleOffset = multiplier * angleStep;
        Vector3 currentDir = Quaternion.Euler(0, 0, currentAngleOffset) * baseAimDir;
        Vector3 positionOffset = perpendicularDir * multiplier * _positionSpread;
        Vector3 spawnPosition = context.Origin.position + positionOffset;

        float angle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        AbilityHitbox hitbox = context.Spawner.Spawn(_hitboxPrefab, spawnPosition, targetRotation);
        
        if (hitbox != null)
        {
            hitbox.transform.localScale = targetScale;
            DamageData damageData = CalculateDamage(context, snapshot);
            hitbox.Initialize(damageData, context.Spawner);
        }
    }
}