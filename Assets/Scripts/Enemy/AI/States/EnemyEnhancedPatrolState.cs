using UnityEngine;

/// <summary>
/// Enhanced patrol state with more interesting behaviors:
/// - Random pauses at patrol points (looking around)
/// - Variable movement speeds
/// - Occasional direction changes
/// - Waiting/investigating behaviors
/// </summary>
public sealed class EnemyEnhancedPatrolState : IState
{
    private enum SubState
    {
        Walking,
        Pausing,
        LookingAround,
        Investigating
    }

    private readonly EnemyContext _ctx;
    private readonly StateMachine _fsm;
    private readonly IPatrolPointProvider _patrolProvider;
    private readonly EnemyPatrolBehaviorConfig _config;

    private SubState _subState;
    private Vector3 _currentTarget;
    private float _timer;
    private float _repathTimer;
    private int _lookAroundSteps;
    private Vector2 _lookDirection;

    public EnemyEnhancedPatrolState(
        EnemyContext ctx,
        StateMachine fsm,
        IPatrolPointProvider patrolProvider,
        EnemyPatrolBehaviorConfig config)
    {
        _ctx = ctx;
        _fsm = fsm;
        _patrolProvider = patrolProvider;
        _config = config ?? new EnemyPatrolBehaviorConfig();
    }

    public void Enter()
    {
        _repathTimer = 0f;
        _subState = SubState.Walking;
        PickNewPoint();
    }

    public void Exit()
    {
        _ctx.Movement.Stop();
    }

    public void Tick(float deltaTime)
    {
        // Always check for combat first
        if (_ctx.Brain.TryGetDesiredCombatState(_fsm.Current, out var desired))
        {
            _fsm.ChangeState(desired);
            return;
        }

        switch (_subState)
        {
            case SubState.Walking:
                TickWalking(deltaTime);
                break;
            case SubState.Pausing:
                TickPausing(deltaTime);
                break;
            case SubState.LookingAround:
                TickLookingAround(deltaTime);
                break;
            case SubState.Investigating:
                TickInvestigating(deltaTime);
                break;
        }
    }

    private void TickWalking(float deltaTime)
    {
        _repathTimer -= deltaTime;

        Vector2 to = (Vector2)(_currentTarget - _ctx.Transform.position);
        float dist = to.magnitude;

        // Arrived at destination
        if (dist <= _config.ArriveDistance)
        {
            _ctx.Movement.Stop();
            DecideNextBehavior();
            return;
        }

        // Repath timeout (stuck or taking too long)
        if (_repathTimer <= 0f)
        {
            PickNewPoint();
            to = (Vector2)(_currentTarget - _ctx.Transform.position);
        }

        _ctx.Movement.SetMoveInput(to.normalized);
    }

    private void TickPausing(float deltaTime)
    {
        _ctx.Movement.Stop();
        _timer -= deltaTime;

        if (_timer <= 0f)
        {
            // After pause, maybe look around or continue walking
            if (Random.value < _config.LookAroundChance)
            {
                StartLookingAround();
            }
            else
            {
                StartWalking();
            }
        }
    }

    private void TickLookingAround(float deltaTime)
    {
        _ctx.Movement.Stop();
        _timer -= deltaTime;

        if (_timer <= 0f)
        {
            _lookAroundSteps--;

            if (_lookAroundSteps > 0)
            {
                // Turn to a new random direction
                _lookDirection = Random.insideUnitCircle.normalized;
                _timer = Random.Range(_config.LookStepDurationMin, _config.LookStepDurationMax);
                
                // Briefly "jiggle" in the look direction to show alertness
                _ctx.Movement.SetMoveInput(_lookDirection * 0.1f);
            }
            else
            {
                // Done looking around, maybe investigate or continue
                if (Random.value < _config.InvestigateChance)
                {
                    StartInvestigating();
                }
                else
                {
                    StartWalking();
                }
            }
        }
    }

    private void TickInvestigating(float deltaTime)
    {
        Vector2 to = (Vector2)(_currentTarget - _ctx.Transform.position);
        float dist = to.magnitude;

        if (dist <= _config.ArriveDistance * 0.5f)
        {
            // Reached investigation point, pause briefly then walk
            _ctx.Movement.Stop();
            _timer -= deltaTime;

            if (_timer <= 0f)
            {
                StartWalking();
            }
        }
        else
        {
            // Move slowly toward investigation point
            _ctx.Movement.SetMoveInput(to.normalized * 0.5f);
            _timer -= deltaTime;

            // Timeout - give up investigating
            if (_timer <= 0f)
            {
                StartWalking();
            }
        }
    }

    private void DecideNextBehavior()
    {
        float roll = Random.value;

        if (roll < _config.PauseChance)
        {
            StartPausing();
        }
        else if (roll < _config.PauseChance + _config.LookAroundChance)
        {
            StartLookingAround();
        }
        else
        {
            StartWalking();
        }
    }

    private void StartWalking()
    {
        _subState = SubState.Walking;
        PickNewPoint();
    }

    private void StartPausing()
    {
        _subState = SubState.Pausing;
        _timer = Random.Range(_config.PauseDurationMin, _config.PauseDurationMax);
    }

    private void StartLookingAround()
    {
        _subState = SubState.LookingAround;
        _lookAroundSteps = Random.Range(_config.LookAroundStepsMin, _config.LookAroundStepsMax + 1);
        _lookDirection = Random.insideUnitCircle.normalized;
        _timer = Random.Range(_config.LookStepDurationMin, _config.LookStepDurationMax);
    }

    private void StartInvestigating()
    {
        _subState = SubState.Investigating;
        
        // Pick a nearby point to "investigate"
        Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(1f, 3f);
        _currentTarget = _ctx.Transform.position + (Vector3)randomOffset;
        _currentTarget.z = _ctx.Transform.position.z;
        
        _timer = _config.InvestigateTimeoutSeconds;
    }

    private void PickNewPoint()
    {
        _repathTimer = _config.RepathSeconds;

        if (_patrolProvider != null && _patrolProvider.TryGetNextPoint(_ctx.Transform.position, out var p))
        {
            _currentTarget = p;
        }
        else
        {
            // Fallback: random direction
            Vector2 random = Random.insideUnitCircle.normalized;
            _currentTarget = _ctx.Transform.position + (Vector3)(random * Random.Range(2f, 5f));
        }

        _currentTarget.z = _ctx.Transform.position.z;
    }
}

/// <summary>
/// Configuration for enhanced patrol behavior. Can be shared across enemies or customized per-type.
/// </summary>
[System.Serializable]
public class EnemyPatrolBehaviorConfig
{
    [Header("Movement")]
    [Tooltip("How close to target before considered 'arrived'")]
    public float ArriveDistance = 0.25f;
    
    [Tooltip("Seconds before picking a new point if stuck")]
    public float RepathSeconds = 3f;

    [Header("Pausing")]
    [Tooltip("Chance to pause after arriving at a point (0-1)")]
    [Range(0f, 1f)]
    public float PauseChance = 0.4f;
    
    public float PauseDurationMin = 0.5f;
    public float PauseDurationMax = 2f;

    [Header("Looking Around")]
    [Tooltip("Chance to look around after pausing (0-1)")]
    [Range(0f, 1f)]
    public float LookAroundChance = 0.3f;
    
    public int LookAroundStepsMin = 2;
    public int LookAroundStepsMax = 4;
    public float LookStepDurationMin = 0.3f;
    public float LookStepDurationMax = 0.8f;

    [Header("Investigating")]
    [Tooltip("Chance to investigate a nearby spot after looking around (0-1)")]
    [Range(0f, 1f)]
    public float InvestigateChance = 0.2f;
    
    public float InvestigateTimeoutSeconds = 3f;
}
