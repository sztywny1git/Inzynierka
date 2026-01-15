using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public enum CasterState
{
    Idle,
    PreCast,
    PostCast
}

[RequireComponent(typeof(ActionConstraintSystem))]
public class AbilityCaster : MonoBehaviour
{
    [SerializeField] private StatSystemConfig _statConfig;
    [SerializeField] private float _inputBufferTime = 0.4f;

    public event Action<string> OnCastAnimationRequired;
    public event Action OnCastInterrupted;

    private IObjectResolver _container;
    private IAbilitySpawner _spawner;
    private IStatsProvider _statsProvider;
    private ILiving _livingEntity;
    private IResourceProvider _resourceProvider;
    private ICooldownProvider _cooldownProvider;
    private ActionConstraintSystem _constraintSystem;
    
    private AbilitySpawnPoint _cachedSpawnPoint;
    
    private CasterState _currentState = CasterState.Idle;
    private IAbility _currentAbility;
    private IAbilityData _currentSnapshot;
    private Vector3 _currentAimLocation;
    private Coroutine _safetyCoroutine;

    private IAbility _bufferedAbility;
    private Vector3 _bufferedAimLocation;
    private float _bufferExpireTimestamp;

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
        _constraintSystem = GetComponent<ActionConstraintSystem>();
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
        HandleCastRequest(_abilities[index], worldPosition);
    }

    private void HandleCastRequest(IAbility ability, Vector3 aimLocation)
    {
        if (ability.IsInstant)
        {
            ExecuteInstant(ability, aimLocation);
            return;
        }

        if (ability.IsPriority)
        {
            if (_currentState != CasterState.Idle)
            {
                InterruptCast();
            }
            StartCastSequence(ability, aimLocation);
            return;
        }

        if (_currentState != CasterState.Idle || !_constraintSystem.CanAbility)
        {
            _bufferedAbility = ability;
            _bufferedAimLocation = aimLocation;
            _bufferExpireTimestamp = Time.time + _inputBufferTime;
            return;
        }

        StartCastSequence(ability, aimLocation);
    }

    private void ExecuteInstant(IAbility ability, Vector3 aimLocation)
    {
        if (!CheckConditions(ability, aimLocation)) return;

        var context = CreateContext(ability.ActionId, aimLocation);
        ConsumeResources(ability, context);
        
        var snapshot = ability.CreateData(_statsProvider, _statConfig);
        ability.Execute(context, snapshot);
    }

    private void StartCastSequence(IAbility ability, Vector3 aimLocation)
    {
        if (!CheckConditions(ability, aimLocation))
        {
            ClearBuffer();
            return;
        }

        ClearBuffer();

        _currentAbility = ability;
        _currentAimLocation = aimLocation;
        _currentSnapshot = ability.CreateData(_statsProvider, _statConfig);

        _currentState = CasterState.PreCast;
        
        _constraintSystem.AddMovementLock();
        _constraintSystem.AddAbilityLock();

        if (_safetyCoroutine != null) StopCoroutine(_safetyCoroutine);
        _safetyCoroutine = StartCoroutine(SafetyNetRoutine(ability.MaxCastDuration));

        if (!string.IsNullOrEmpty(ability.AnimationTriggerName))
        {
            OnCastAnimationRequired?.Invoke(ability.AnimationTriggerName);
        }
        else
        {
            OnAnimAttackPoint(); 
        }
    }

    public void OnAnimAttackPoint()
    {
        if (_currentState != CasterState.PreCast || _currentAbility == null) return;

        var context = CreateContext(_currentAbility.ActionId, _currentAimLocation);
        
        ConsumeResources(_currentAbility, context);
        _currentAbility.Execute(context, _currentSnapshot);

        _currentState = CasterState.PostCast;
        _constraintSystem.RemoveMovementLock();
    }

    public void OnAnimFinish()
    {
        if (_currentState == CasterState.Idle) return;
        ResetState();
    }

    public void InterruptCast()
    {
        if (_currentState == CasterState.Idle) return;

        if (_safetyCoroutine != null) StopCoroutine(_safetyCoroutine);
        
        ResetLocksBasedOnState();
        ClearBuffer();
        
        _currentState = CasterState.Idle;
        _currentAbility = null;
        _currentSnapshot = null;
        
        OnCastInterrupted?.Invoke();
    }

    private void ResetState()
    {
        if (_safetyCoroutine != null) StopCoroutine(_safetyCoroutine);
        
        ResetLocksBasedOnState();
        
        _currentState = CasterState.Idle;
        _currentAbility = null;
        _currentSnapshot = null;

        ProcessBufferedInput();
    }

    private void ProcessBufferedInput()
    {
        if (_bufferedAbility != null && Time.time < _bufferExpireTimestamp)
        {
            if (_constraintSystem.CanAbility)
            {
                StartCastSequence(_bufferedAbility, _bufferedAimLocation);
            }
        }
        else
        {
            ClearBuffer();
        }
    }

    private void ClearBuffer()
    {
        _bufferedAbility = null;
        _bufferExpireTimestamp = 0f;
    }

    private void ResetLocksBasedOnState()
    {
        if (_currentState == CasterState.PreCast)
        {
            _constraintSystem.RemoveMovementLock();
            _constraintSystem.RemoveAbilityLock();
        }
        else if (_currentState == CasterState.PostCast)
        {
            _constraintSystem.RemoveAbilityLock();
        }
    }

    private IEnumerator SafetyNetRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        ResetState();
    }

    private bool CheckConditions(IAbility ability, Vector3 aimLocation)
    {
        var context = CreateContext(ability.ActionId, aimLocation);
        foreach (var condition in ability.Conditions)
        {
            if (!condition.CanBeUsed(context)) return false;
        }
        return true;
    }

    private void ConsumeResources(IAbility ability, AbilityContext context)
    {
        foreach (var condition in ability.Conditions)
        {
            condition.OnUse(context);
        }
    }

    private AbilityContext CreateContext(ActionIdentifier actionId, Vector3 aimLocation)
    {
        Transform currentOrigin = _cachedSpawnPoint ? _cachedSpawnPoint.transform : transform;
        return new AbilityContext(
            gameObject, _spawner, _resourceProvider, _cooldownProvider, _statsProvider, _statConfig,
            currentOrigin, aimLocation, actionId
        );
    }

    private void HandleDeath()
    {
        InterruptCast();
        ClearBuffer();
    }
}