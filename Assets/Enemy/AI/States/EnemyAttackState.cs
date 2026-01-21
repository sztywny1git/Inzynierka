using UnityEngine;

public sealed class EnemyAttackState : IState
{
    private readonly EnemyContext _ctx;
    private readonly StateMachine _fsm;
    private readonly float _attackDistance;
    private readonly IState _fallback;
    private EnemyAnimator _anim;

    private float _nextBlockedLogTime;

    public EnemyAttackState(EnemyContext ctx, StateMachine fsm, float attackDistance, IState fallback)
    {
        _ctx = ctx;
        _fsm = fsm;
        _attackDistance = Mathf.Max(0.05f, attackDistance);
        _fallback = fallback;
    }

    public void Enter()
    {
        _ctx.Movement.Stop();
        _anim ??= _ctx.Brain.GetComponent<EnemyAnimator>();
        _nextBlockedLogTime = 0f;
    }

    public void Exit() { }

    public void Tick(float deltaTime)
    {
        if (_ctx.Brain.TryGetDesiredCombatState(_fsm.Current, out var desired))
        {
            _fsm.ChangeState(desired);
            return;
        }

        var target = _ctx.TargetProvider.Target;
        if (target == null)
        {
            _fsm.ChangeState(_fallback);
            return;
        }

        float dist = _ctx.MeleeAttack != null
            ? _ctx.MeleeAttack.GetDistanceToTarget(target)
            : Vector2.Distance(_ctx.Transform.position, target.position);

        float effectiveAttackDistance = _attackDistance;
        if (_ctx.MeleeAttack != null)
        {
            effectiveAttackDistance = Mathf.Min(effectiveAttackDistance, _ctx.MeleeAttack.AttackRange);
        }

        if (dist > effectiveAttackDistance)
        {
            _fsm.ChangeState(_fallback);
            return;
        }

        _ctx.Movement.Stop();

        bool attacked = _ctx.MeleeAttack.TryAttack(target);
        if (attacked)
        {
            _anim?.TriggerAttack();
            if (_ctx.Brain != null && _ctx.Brain.DebugLogging)
            {
                Debug.Log($"[EnemyAttackState] '{_ctx.Brain.name}' ATTACK executed on '{target.name}'.", _ctx.Brain);
            }
            return;
        }

        if (_ctx.Brain != null && _ctx.Brain.DebugLogging && Time.time >= _nextBlockedLogTime)
        {
            bool canAttack = _ctx.MeleeAttack.CanAttackNow();
            Debug.Log(
                $"[EnemyAttackState] '{_ctx.Brain.name}' cannot attack yet (cooldown={(!canAttack)}). dist={dist:0.###} effectiveDist={effectiveAttackDistance:0.###}",
                _ctx.Brain);
            _nextBlockedLogTime = Time.time + 0.75f;
        }
    }
}
