using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class AbilityCaster : MonoBehaviour
{
    [SerializeField] private StatSystemConfig _statConfig;

    public event Action<string> OnCastAnimationRequired;
    public event Action OnCastInterrupted;

    private IObjectResolver _container; 
    private IAbilitySpawner _spawner;
    private IStatsProvider _statsProvider;
    private ILiving _livingEntity;
    private IResourceProvider _resourceProvider;
    private ICooldownProvider _cooldownProvider;
    
    private AbilitySpawnPoint _cachedSpawnPoint;
    private IAbility _pendingAbility;
    private IAbilityData _pendingData; 
    private Vector3 _pendingAimLocation;
    
    private List<IAbility> _abilities = new List<IAbility>();

    [Inject]
    public void Construct(IObjectResolver container, IAbilitySpawner spawner)
    {
        _container = container;
        _spawner = spawner;
    }

    private void Awake()
    {
        _statsProvider = GetComponent<IStatsProvider>();
        _livingEntity = GetComponent<ILiving>();
        _resourceProvider = GetComponent<IResourceProvider>();
        _cooldownProvider = GetComponent<ICooldownProvider>();
        _cachedSpawnPoint = GetComponentInChildren<AbilitySpawnPoint>();
    }

    private void OnEnable()
    {
        if (_livingEntity != null) _livingEntity.Death += HandleDeath;
    }

    private void OnDisable()
    {
        if (_livingEntity != null) _livingEntity.Death -= HandleDeath;
    }

    public void Initialize(IEnumerable<IAbility> abilities)
    {
        _abilities.Clear();
        if (abilities != null) _abilities.AddRange(abilities);
    }

    public void RequestAbility(int index, Vector3 worldPosition)
    {
        if (index < 0 || index >= _abilities.Count) return;
        if (_livingEntity != null && !_livingEntity.isAlive) return;
        
        worldPosition.z = 0; 
        UseAbility(_abilities[index], worldPosition);
    }

    private void UseAbility(IAbility ability, Vector3 aimLocation)
    {
        if (_pendingAbility != null) return;

        var context = new AbilityContext(
            gameObject, _spawner, _resourceProvider, _cooldownProvider, _statsProvider, _statConfig,
            transform, aimLocation, ability.ActionId
        );

        foreach (var condition in ability.Conditions)
        {
            if (!condition.CanBeUsed(context)) return;
        }

        foreach (var condition in ability.Conditions)
        {
            condition.OnUse(context);
        }

        _pendingAbility = ability;
        _pendingAimLocation = aimLocation;
        
        if (_statsProvider != null && _statConfig != null)
        {
            _pendingData = ability.CreateData(_statsProvider, _statConfig);
        }
        else
        {
            _pendingData = null;
        }
        
        if (ability.ExecuteImmediately)
        {
            ReleaseSpell();
            
            if (!string.IsNullOrEmpty(ability.AnimationTriggerName))
            {
                OnCastAnimationRequired?.Invoke(ability.AnimationTriggerName);
            }
        }
        else
        {
            if (string.IsNullOrEmpty(ability.AnimationTriggerName))
            {
                ReleaseSpell();
            }
            else
            {
                OnCastAnimationRequired?.Invoke(ability.AnimationTriggerName);
            }
        }
    }

    public void ReleaseSpell()
    {
        if (_livingEntity != null && !_livingEntity.isAlive)
        {
            HandleDeath();
            return;
        }

        if (_pendingAbility == null) return;

        Transform currentOrigin = _cachedSpawnPoint ? _cachedSpawnPoint.transform : transform;

        var finalContext = new AbilityContext(
            gameObject, _spawner, _resourceProvider, _cooldownProvider, _statsProvider, _statConfig,
            currentOrigin, _pendingAimLocation, _pendingAbility.ActionId
        );

        _pendingAbility.Execute(finalContext, _pendingData);
        
        _pendingAbility = null;
        _pendingData = null;
    }

    public void CancelCast()
    {
        _pendingAbility = null;
        _pendingData = null;
        OnCastInterrupted?.Invoke();
    }

    private void HandleDeath()
    {
        _pendingAbility = null;
        _pendingData = null;
        OnCastInterrupted?.Invoke();
    }
}