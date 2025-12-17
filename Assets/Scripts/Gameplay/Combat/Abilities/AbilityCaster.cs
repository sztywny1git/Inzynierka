using UnityEngine;
using System.Collections.Generic;
using VContainer;

public class AbilityCaster : MonoBehaviour
{
    private class AbilitySlot { public Ability Ability; }

    [SerializeField] private string actionStateTag = "Action";
    [SerializeField] private StatDefinition attackSpeedStatDef;
    
    private readonly List<AbilitySlot> _slots = new List<AbilitySlot>();
    
    private IObjectResolver _diContainer;
    private IStatsProvider _stats;
    private Animator _animator;

    private AbilitySlot _abilityToExecute;
    private Vector2 _lastDirection;
    
    private static readonly int AttackSpeedMultiplierHash = Animator.StringToHash("AttackSpeedMultiplier");

    [Inject]
    public void Construct(IObjectResolver diContainer)
    {
        _diContainer = diContainer;
    }
    
    private void Awake()
    {
        _stats = GetComponent<IStatsProvider>();
        _animator = GetComponentInChildren<Animator>();
    }
    
    public void RequestAbility(int slotIndex, Vector2 direction)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return;
        
        var slot = _slots[slotIndex];
        if (slot == null || slot.Ability == null) return;
        
        if (_animator != null && _animator.GetCurrentAnimatorStateInfo(0).IsTag(actionStateTag))
        {
            return;
        }
        
        var context = new AbilityContext(
            owner: this.gameObject,
            origin: transform,
            direction: direction,
            stats: _stats,
            diContainer: _diContainer,
            ability: slot.Ability,
            resourceSystem: GetComponent<IResourceSystem>()
        );

        foreach (var condition in slot.Ability.Conditions)
        {
            if (!condition.CanBeUsed(context)) return;
        }

        _abilityToExecute = slot;
        _lastDirection = direction;

        if (_animator != null)
        {
            float finalAttackSpeed = _stats.GetFinalStatValue(attackSpeedStatDef);
            _animator.SetFloat(AttackSpeedMultiplierHash, finalAttackSpeed > 0 ? finalAttackSpeed : 1f);
            _animator.SetTrigger(slot.Ability.AnimationTrigger);
        }
    }

    public void HandleAbilityFrame()
    {
        if (_abilityToExecute == null || _abilityToExecute.Ability == null) return;

        var context = new AbilityContext(
            owner: this.gameObject,
            origin: transform,
            direction: _lastDirection,
            stats: _stats,
            diContainer: _diContainer,
            ability: _abilityToExecute.Ability,
            resourceSystem: GetComponent<IResourceSystem>()
        );
        
        _abilityToExecute.Ability.Execute(context);
        
        foreach (var condition in _abilityToExecute.Ability.Conditions)
        {
            condition.OnUse(context);
        }

        _abilityToExecute = null;
    }
    
    public void SetAbilities(List<Ability> abilities)
    {
        _slots.Clear();
        if (abilities != null)
        {
            foreach (var ability in abilities)
            {
                _slots.Add(new AbilitySlot { Ability = ability });
            }
        }
    }
}