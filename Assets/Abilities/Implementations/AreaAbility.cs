using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Area Ability")]
public class AreaAbility : DamageAbility, IStepAbility
{
    [Header("Runner Configuration")]
    [SerializeField] private AbilityRunner _runnerPrefab;

    [Header("Area Configuration")]
    [SerializeField] private AbilityHitbox _hitboxPrefab;
    [SerializeField] private float _radiusFromCenter = 1.5f;
    [SerializeField] private float _spawnDelay = 0.1f;

    public override void Execute(AbilityContext context, AbilitySnapshot snapshot)
    {
        int count = AttackCount;
        List<Vector3> spawnPositions = new List<Vector3>(count);

        if (count == 1)
        {
            spawnPositions.Add(context.AimLocation);
        }
        else
        {
            float angleStep = 360f / count;
            float currentAngle = Random.Range(0f, 360f);

            for (int i = 0; i < count; i++)
            {
                float radians = currentAngle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * _radiusFromCenter;
                spawnPositions.Add(context.AimLocation + offset);
                currentAngle += angleStep;
            }

            for (int i = 0; i < spawnPositions.Count; i++)
            {
                Vector3 temp = spawnPositions[i];
                int randomIndex = Random.Range(i, spawnPositions.Count);
                spawnPositions[i] = spawnPositions[randomIndex];
                spawnPositions[randomIndex] = temp;
            }
        }

        if (_runnerPrefab != null)
        {
            var runner = context.Spawner.Spawn(_runnerPrefab, context.Origin.position, Quaternion.identity);
            runner.Initialize(this, context, snapshot, count, _spawnDelay, spawnPositions);
        }
    }

    public void OnRunnerStep(AbilityContext context, AbilitySnapshot snapshot, int index, AbilityRunner runner)
    {
        var positions = runner.GetState<List<Vector3>>();
        if (positions == null || index >= positions.Count) return;

        Vector3 spawnPosition = positions[index];
        spawnPosition.z = 0;

        AbilityHitbox hitbox = context.Spawner.Spawn(_hitboxPrefab, spawnPosition, Quaternion.identity);
        
        if (hitbox != null)
        {
            DamageData damageData = CalculateDamage(context, snapshot);
            hitbox.Initialize(damageData, context.Spawner);
        }
    }
}