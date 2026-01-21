using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(menuName = "Abilities/Melee Ability")]
public class MeleeAbility : DamageAbility
{
    [Header("Melee Configuration")]
    [SerializeField] private AbilityHitbox _hitboxPrefab;
    [SerializeField] private float _spawnDelay = 0.1f;
    [SerializeField] private float _spreadAngle = 45f;
    [SerializeField] private float _positionSpread = 0.5f;

    public override async UniTask Execute(AbilityContext context, AbilitySnapshot snapshot)
    {
        var token = context.Instigator.GetCancellationTokenOnDestroy();

        Vector3 baseAimDir = (context.AimLocation - context.Origin.position).normalized;
        baseAimDir.z = 0;

        int count = Mathf.Max(1, _attackCount);
        float angleStep = (count > 1) ? _spreadAngle / (count - 1) : 0f;

        Vector3 targetScale = Vector3.one;
        if (baseAimDir.x < 0)
        {
            targetScale.y = -1f;
        }

        Vector3 perpendicularDir = new Vector3(-baseAimDir.y, baseAimDir.x, 0);

        for (int i = 0; i < count; i++)
        {
            DamageData damageData = CalculateDamage(context, snapshot);

            float multiplier = 0f;
            if (i > 0)
            {
                multiplier = (i % 2 != 0) ? ((i + 1) / 2f) : -((i) / 2f);
            }

            float currentAngleOffset = multiplier * angleStep;
            Vector3 currentDir = Quaternion.Euler(0, 0, currentAngleOffset) * baseAimDir;

            Vector3 positionOffset = perpendicularDir * multiplier * _positionSpread;
            Vector3 spawnPosition = context.Origin.position + positionOffset;

            float angle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

            AbilityHitbox hitbox = context.Spawner.Spawn(_hitboxPrefab, spawnPosition, targetRotation);
            
            hitbox.transform.localScale = targetScale;
            hitbox.Initialize(damageData, context.Spawner);

            if (_spawnDelay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_spawnDelay), cancellationToken: token);
            }
        }
    }
}