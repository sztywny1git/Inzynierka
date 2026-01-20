using UnityEngine;

public sealed class EnemySmartCombatState : IState
{
    private enum Phase { Positioning, Strafing, Attacking, Dodging, Recovery }

    private const float DODGE_DURATION = 0.3f;
    private const float DODGE_COOLDOWN = 1.5f;
    private const float STRAFE_CHANGE_TIME = 2f;
    private const float ATTACK_RECOVERY = 0.2f;
    private const float MIN_ALLY_DISTANCE = 1f;
    private const float ATTACK_CHANCE_PER_SECOND = 2.5f;

    private readonly EnemyContext _ctx;
    private readonly StateMachine _fsm;
    private readonly ThreatDetector _threats;
    private readonly IState _chaseState;
    private readonly IState _patrolState;
    private readonly float _attackRange;
    private readonly float _preferredDistance;

    private Phase _phase;
    private float _phaseTimer;
    private float _strafeDir;
    private float _nextStrafeChange;
    private float _lastDodgeTime;
    private Vector2 _dodgeDirection;
    private EnemyAnimator _anim;

    public EnemySmartCombatState(
        EnemyContext ctx,
        StateMachine fsm,
        ThreatDetector threats,
        IState chaseState,
        IState patrolState,
        float attackRange)
    {
        _ctx = ctx;
        _fsm = fsm;
        _threats = threats;
        _chaseState = chaseState;
        _patrolState = patrolState;
        _attackRange = attackRange;
        _preferredDistance = Mathf.Max(0.5f, attackRange * 0.7f);
    }

    public void Enter()
    {
        _phase = Phase.Positioning;
        _phaseTimer = 0f;
        _strafeDir = Random.value > 0.5f ? 1f : -1f;
        _nextStrafeChange = Time.time + Random.Range(0.8f, STRAFE_CHANGE_TIME);
        _anim ??= _ctx.Brain.GetComponent<EnemyAnimator>();
    }

    public void Exit()
    {
        _ctx.Movement.Stop();
    }

    public void Tick(float deltaTime)
    {
        var target = _ctx.TargetProvider.Target;
        if (target == null)
        {
            _fsm.ChangeState(_patrolState);
            return;
        }

        float dist = GetDistanceToTarget(target);

        if (_phase != Phase.Dodging && _phase != Phase.Attacking && ShouldDodge())
        {
            StartDodge();
            return;
        }

        if (dist > _attackRange * 2f && _phase != Phase.Dodging)
        {
            _fsm.ChangeState(_chaseState);
            return;
        }

        switch (_phase)
        {
            case Phase.Positioning:
                TickPositioning(target, dist, deltaTime);
                break;
            case Phase.Strafing:
                TickStrafing(target, dist, deltaTime);
                break;
            case Phase.Attacking:
                TickAttacking(target);
                break;
            case Phase.Dodging:
                TickDodging(deltaTime);
                break;
            case Phase.Recovery:
                TickRecovery(deltaTime);
                break;
        }
    }

    private void TickPositioning(Transform target, float dist, float deltaTime)
    {
        Vector2 toTarget = ((Vector2)target.position - (Vector2)_ctx.Transform.position).normalized;
        
        if (CanAttack() && dist <= _attackRange)
        {
            _phase = Phase.Attacking;
            return;
        }

        Vector2 moveDir = Vector2.zero;

        if (dist > _attackRange)
        {
            moveDir = toTarget;
        }
        else if (dist < _preferredDistance * 0.5f)
        {
            moveDir = -toTarget * 0.5f;
        }
        else
        {
            _phase = Phase.Strafing;
            return;
        }

        moveDir += GetCrowdAvoidance() * 0.4f;
        _ctx.Movement.SetMoveInput(moveDir.normalized);
    }

    private void TickStrafing(Transform target, float dist, float deltaTime)
    {
        if (CanAttack() && dist <= _attackRange)
        {
            if (Random.value < deltaTime * ATTACK_CHANCE_PER_SECOND)
            {
                _phase = Phase.Attacking;
                return;
            }
        }

        if (Time.time >= _nextStrafeChange)
        {
            _strafeDir *= -1f;
            _nextStrafeChange = Time.time + Random.Range(0.8f, STRAFE_CHANGE_TIME);
        }

        Vector2 toTarget = ((Vector2)target.position - (Vector2)_ctx.Transform.position).normalized;
        Vector2 strafeDir = Vector2.Perpendicular(toTarget) * _strafeDir;

        float distError = dist - _preferredDistance;
        Vector2 distCorrection = toTarget * Mathf.Clamp(distError, -0.3f, 0.6f);

        Vector2 moveDir = strafeDir * 0.7f + distCorrection + GetCrowdAvoidance() * 0.2f;
        
        _ctx.Movement.SetMoveInput(moveDir.normalized);

        if (dist > _attackRange * 1.5f)
        {
            _phase = Phase.Positioning;
        }
    }

    private void TickAttacking(Transform target)
    {
        _ctx.Movement.Stop();

        if (_ctx.MeleeAttack.TryAttack(target))
        {
            _anim?.TriggerAttack();
            
            _phase = Phase.Recovery;
            _phaseTimer = ATTACK_RECOVERY;

            if (_ctx.Brain.DebugLogging)
            {
                Debug.Log($"[SmartCombat] '{_ctx.Brain.name}' attacked '{target.name}'!", _ctx.Brain);
            }
        }
        else
        {
            _phase = Phase.Strafing;
        }
    }

    private void TickDodging(float deltaTime)
    {
        _phaseTimer -= deltaTime;
        
        _ctx.Movement.SetMoveInput(_dodgeDirection.normalized);

        if (_phaseTimer <= 0f)
        {
            _phase = Phase.Positioning;
        }
    }

    private void TickRecovery(float deltaTime)
    {
        _phaseTimer -= deltaTime;

        var target = _ctx.TargetProvider.Target;
        if (target != null)
        {
            Vector2 away = ((Vector2)_ctx.Transform.position - (Vector2)target.position).normalized;
            _ctx.Movement.SetMoveInput(away * 0.4f);
        }

        if (_phaseTimer <= 0f)
        {
            _phase = Phase.Strafing;
        }
    }

    private bool ShouldDodge()
    {
        if (_threats == null) return false;
        if (Time.time < _lastDodgeTime + DODGE_COOLDOWN) return false;
        return _threats.ShouldDodgeNow(0.35f);
    }

    private void StartDodge()
    {
        _phase = Phase.Dodging;
        _phaseTimer = DODGE_DURATION;
        _lastDodgeTime = Time.time;

        _dodgeDirection = _threats?.GetBestDodgeDirection() ?? Vector2.zero;
        if (_dodgeDirection.sqrMagnitude < 0.01f)
        {
            _dodgeDirection = _strafeDir > 0 ? Vector2.right : Vector2.left;
        }

        if (_ctx.Brain.DebugLogging)
        {
            Debug.Log($"[SmartCombat] '{_ctx.Brain.name}' DODGE! dir={_dodgeDirection}", _ctx.Brain);
        }
    }

    private bool CanAttack()
    {
        return _ctx.MeleeAttack != null && _ctx.MeleeAttack.CanAttackNow();
    }

    private float GetDistanceToTarget(Transform target)
    {
        return _ctx.MeleeAttack != null
            ? _ctx.MeleeAttack.GetDistanceToTarget(target)
            : Vector2.Distance(_ctx.Transform.position, target.position);
    }

    private Vector2 GetCrowdAvoidance()
    {
        Vector2 avoidance = Vector2.zero;
        var enemies = Object.FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            if (enemy == _ctx.Brain) continue;

            Vector2 toOther = (Vector2)enemy.transform.position - (Vector2)_ctx.Transform.position;
            float dist = toOther.magnitude;

            if (dist < MIN_ALLY_DISTANCE && dist > 0.01f)
            {
                float strength = 1f - (dist / MIN_ALLY_DISTANCE);
                avoidance -= toOther.normalized * strength;
            }
        }

        return avoidance;
    }
}
