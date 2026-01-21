using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(menuName = "Abilities/Area Ability")]
public class AreaAbility : DamageAbility
{
    [Header("Area Configuration")]
    [SerializeField] private AbilityHitbox _hitboxPrefab;
    [SerializeField] private float _radiusFromCenter = 1.5f;
    [SerializeField] private float _spawnDelay = 0.1f;

    public override async UniTask Execute(AbilityContext context, AbilitySnapshot snapshot)
    {
        var token = context.Instigator.GetCancellationTokenOnDestroy();

        int count = Mathf.Max(1, _attackCount);
        List<Vector3> spawnPositions = new List<Vector3>();

        if (count == 1)
        {
            spawnPositions.Add(context.AimLocation);
        }
        else
        {
            float angleStep = 360f / count;
            float currentAngle = UnityEngine.Random.Range(0f, 360f);

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
                int randomIndex = UnityEngine.Random.Range(i, spawnPositions.Count);
                spawnPositions[i] = spawnPositions[randomIndex];
                spawnPositions[randomIndex] = temp;
            }
        }

        for (int i = 0; i < count; i++)
        {
            DamageData damageData = CalculateDamage(context, snapshot);

            Vector3 spawnPosition = spawnPositions[i];
            spawnPosition.z = 0;

            AbilityHitbox hitbox = context.Spawner.Spawn(_hitboxPrefab, spawnPosition, Quaternion.identity);
            hitbox.Initialize(damageData, context.Spawner);

            if (_spawnDelay > 0 && i < count - 1)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_spawnDelay), cancellationToken: token);
            }
        }
    }
}