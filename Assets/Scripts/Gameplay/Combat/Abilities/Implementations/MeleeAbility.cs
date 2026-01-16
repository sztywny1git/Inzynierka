using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee Ability")]
public class MeleeAbility : DamageAbility
{
    [Header("Melee Configuration")]
    [SerializeField] private AbilityHitbox _hitboxPrefab;
    [SerializeField] private float _spawnDelay = 0.1f;
    [SerializeField] private float _spreadAngle = 45f;

    public override void Execute(AbilityContext context, AbilitySnapshot snapshot)
    {
        MonoBehaviour runner = context.Instigator.GetComponent<MonoBehaviour>();
        if (runner != null)
        {
            runner.StartCoroutine(SpawnRoutine(context, snapshot));
        }
    }

    private IEnumerator SpawnRoutine(AbilityContext context, AbilitySnapshot snapshot)
    {
        int count = Mathf.Max(1, _attackCount);
        
        float angleStep = count > 1 ? _spreadAngle / (count - 1) : 0;
        float currentAngle = count > 1 ? -_spreadAngle / 2f : 0;

        for (int i = 0; i < count; i++)
        {
            DamageData damageData = CalculateDamage(context, snapshot);
            
            Quaternion rotation = context.Origin.rotation * Quaternion.Euler(0, 0, currentAngle);
            
            AbilityHitbox hitbox = Instantiate(_hitboxPrefab, context.Origin.position, rotation);
            hitbox.Initialize(damageData);

            currentAngle += angleStep;

            if (_spawnDelay > 0)
            {
                yield return new WaitForSeconds(_spawnDelay);
            }
        }
    }
}