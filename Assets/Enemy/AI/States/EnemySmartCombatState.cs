using UnityEngine;

public sealed class EnemySmartCombatState : IState
{
    private enum Phase { Positioning, Strafing, Attacking, Dodging, Recovery }
    private enum RecoveryType { Retreat, SideStep, AggressiveHold } // Nowy enum dla różnorodności

    private const float DODGE_DURATION = 0.3f;
    private const float DODGE_COOLDOWN = 1.5f;
    private const float STRAFE_CHANGE_TIME = 2f;
    
    // Czas trwania całej animacji (blokada inputu)
    private const float ATTACK_ANIMATION_DURATION = 0.6f; 
    // NOWE: Czas "doskoku". Przez pierwsze 0.25s ataku wróg wciąż przesuwa się do przodu
    private const float ATTACK_LUNGE_DURATION = 0.25f; 
    // Siła doskoku (mnożnik prędkości)
    private const float LUNGE_SPEED_MULTIPLIER = 1.5f;

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
    private RecoveryType _currentRecoveryType; // Przechowuje wylosowane zachowanie po ataku
    private float _phaseTimer;
    private float _strafeDir;
    private float _nextStrafeChange;
    private float _lastDodgeTime;
    private Vector2 _dodgeDirection;
    private Vector2 _attackLungeDirection; // Kierunek doskoku
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
        _preferredDistance = Mathf.Max(0.5f, attackRange * 0.9f); 
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

        float exitThreshold = _ctx.Brain.ChaseDistance + 2.0f;
        
        if (dist > exitThreshold && _phase != Phase.Dodging && _phase != Phase.Attacking)
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
                TickAttacking(target, deltaTime);
                break;
            case Phase.Dodging:
                TickDodging(deltaTime);
                break;
            case Phase.Recovery:
                TickRecovery(target, deltaTime);
                break;
        }
    }

    private void TickPositioning(Transform target, float dist, float deltaTime)
    {
        Vector2 toTarget = ((Vector2)target.position - (Vector2)_ctx.Transform.position).normalized;
        
        if (CanAttack() && dist <= _attackRange)
        {
            StartAttackSequence(toTarget);
            return;
        }

        Vector2 moveDir;
        if (dist > _attackRange * 0.9f) 
        {
            moveDir = toTarget; 
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
        Vector2 toTarget = ((Vector2)target.position - (Vector2)_ctx.Transform.position).normalized;

        if (CanAttack() && dist <= _attackRange)
        {
            if (Random.value < deltaTime * ATTACK_CHANCE_PER_SECOND)
            {
                StartAttackSequence(toTarget);
                return;
            }
        }

        if (Time.time >= _nextStrafeChange)
        {
            _strafeDir *= -1f;
            _nextStrafeChange = Time.time + Random.Range(0.8f, STRAFE_CHANGE_TIME);
        }

        Vector2 strafeDir = Vector2.Perpendicular(toTarget) * _strafeDir;
        float distError = dist - _preferredDistance;
        Vector2 distCorrection = toTarget * Mathf.Clamp(distError, -1f, 1f); 
        Vector2 moveDir = strafeDir * 0.6f + distCorrection + GetCrowdAvoidance() * 0.2f;
        
        _ctx.Movement.SetMoveInput(moveDir.normalized);

        if (dist > _attackRange * 1.5f)
        {
            _phase = Phase.Positioning;
        }
    }

    private void StartAttackSequence(Vector2 dirToTarget)
    {
        _phase = Phase.Attacking;
        _phaseTimer = 0f;
        
        // Zapisujemy kierunek, w którym wróg ma zrobić "doskok"
        _attackLungeDirection = dirToTarget;
    }

    private void TickAttacking(Transform target, float deltaTime)
    {
        // Klatka 0: Inicjalizacja ataku
        if (_phaseTimer == 0f)
        {
            if (_ctx.MeleeAttack.TryAttack(target))
            {
                _anim?.TriggerAttack();
                _phaseTimer = ATTACK_ANIMATION_DURATION;
            }
            else
            {
                _phase = Phase.Strafing;
                return;
            }
        }

        // Obliczamy ile czasu minęło od początku ataku
        float timeElapsed = ATTACK_ANIMATION_DURATION - _phaseTimer;

        // POPRAWKA 1: LUNGE (Doskok)
        // Jeśli jesteśmy w początkowej fazie ataku, przesuwamy wroga do przodu.
        if (timeElapsed < ATTACK_LUNGE_DURATION)
        {
            // Możemy lekko korygować kierunek do celu, żeby doskok był celny
            if (target != null)
            {
                 Vector2 currentDir = ((Vector2)target.position - (Vector2)_ctx.Transform.position).normalized;
                 // Lerpujemy kierunek dla płynności
                 _attackLungeDirection = Vector2.Lerp(_attackLungeDirection, currentDir, deltaTime * 10f);
            }
            
            // Ruch z impulsem prędkości
            _ctx.Movement.SetMoveInput(_attackLungeDirection * LUNGE_SPEED_MULTIPLIER);
        }
        else
        {
            // Koniec doskoku - wróg staje w miejscu na moment uderzenia
            _ctx.Movement.Stop();
        }

        // Odliczanie czasu
        _phaseTimer -= deltaTime;

        // Koniec animacji -> Losowanie co robić dalej
        if (_phaseTimer <= 0f)
        {
            DecideNextMove();
        }
    }

    private void DecideNextMove()
    {
        _phase = Phase.Recovery;
        
        // POPRAWKA 2: Losowość zachowań po ataku
        float roll = Random.value;

        if (roll < 0.5f) 
        {
            // 50% - Standardowa ucieczka (daje oddech graczowi)
            _currentRecoveryType = RecoveryType.Retreat;
            _phaseTimer = Random.Range(0.5f, 1.0f); // Losowy czas
        }
        else if (roll < 0.8f)
        {
            // 30% - Side Step (krążenie, utrudnia trafienie wroga)
            _currentRecoveryType = RecoveryType.SideStep;
            _strafeDir = Random.value > 0.5f ? 1f : -1f; // Losowy kierunek
            _phaseTimer = Random.Range(0.4f, 0.8f);
        }
        else
        {
            // 20% - Agresywne (zostaje blisko, gotowy do kolejnego ataku)
            _currentRecoveryType = RecoveryType.AggressiveHold;
            _phaseTimer = Random.Range(0.2f, 0.5f); // Bardzo krótka przerwa
        }
    }

    private void TickRecovery(Transform target, float deltaTime)
    {
        _phaseTimer -= deltaTime;

        if (target != null)
        {
            Vector2 toTarget = ((Vector2)target.position - (Vector2)_ctx.Transform.position).normalized;

            switch (_currentRecoveryType)
            {
                case RecoveryType.Retreat:
                    // Ucieczka do tyłu (jak wcześniej)
                    _ctx.Movement.SetMoveInput(-toTarget * 0.4f);
                    break;

                case RecoveryType.SideStep:
                    // Szybki unik w bok
                    Vector2 side = Vector2.Perpendicular(toTarget) * _strafeDir;
                    _ctx.Movement.SetMoveInput(side * 0.6f);
                    break;

                case RecoveryType.AggressiveHold:
                    // Minimalny ruch lub powolne napieranie (presja)
                    _ctx.Movement.SetMoveInput(toTarget * 0.1f);
                    break;
            }
        }

        if (_phaseTimer <= 0f)
        {
            // Po recovery zazwyczaj wracamy do Strafing lub Positioning
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