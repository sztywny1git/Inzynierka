using UnityEngine;

public class ActionConstraintSystem : MonoBehaviour
{
    private int _movementLocks;
    private int _abilityLocks;

    public bool CanMove => _movementLocks <= 0;
    public bool CanAbility => _abilityLocks <= 0;

    public void AddMovementLock() => _movementLocks++;
    public void RemoveMovementLock() => _movementLocks = Mathf.Max(0, _movementLocks - 1);

    public void AddAbilityLock() => _abilityLocks++;
    public void RemoveAbilityLock() => _abilityLocks = Mathf.Max(0, _abilityLocks - 1);

    public void ResetLocks()
    {
        _movementLocks = 0;
        _abilityLocks = 0;
    }
}