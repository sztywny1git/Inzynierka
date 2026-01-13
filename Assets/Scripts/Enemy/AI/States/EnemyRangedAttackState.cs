using UnityEngine;

public sealed class EnemyRangedAttackState : IState
{
    private readonly EnemyContext _ctx;
    private readonly StateMachine _fsm;
    private readonly EnemyRangedAttack _ranged;
    private readonly float _rangedDistance;
    private readonly float _preferredMinDistance;

    private readonly IState _melee;
    private readonly IState _chase;

    private EnemyAnimator _anim;

    public EnemyRangedAttackState(
        EnemyContext ctx,
        StateMachine fsm,
        EnemyRangedAttack ranged,
        float rangedDistance,
        float preferredMinDistance,
        IState melee,
        IState chase)
    {
        _ctx = ctx;
        _fsm = fsm;
        _ranged = ranged;
        _rangedDistance = Mathf.Max(0.1f, rangedDistance);
        _preferredMinDistance = Mathf.Max(0f, preferredMinDistance);
        _melee = melee;
        _chase = chase;
    }

    public void Enter()
    {
        _anim ??= _ctx.Brain.GetComponent<EnemyAnimator>();
    }

    public void Exit() { }

    public void Tick(float deltaTime)
    {
        var target = _ctx.TargetProvider.Target;
        if (target == null)
        {
            _fsm.ChangeState(_chase);
            return;
        }

        float dist = Vector2.Distance(_ctx.Transform.position, target.position);

        // If too close, switch to melee.
        if (_melee != null && dist <= _ctx.MeleeAttack.AttackRange)
        {
            _fsm.ChangeState(_melee);
            return;
        }

        // If too far, chase.
        if (dist > _rangedDistance)
        {
            _fsm.ChangeState(_chase);
            return;
        }

        // Maintain a bit of distance.
        if (_preferredMinDistance > 0f && dist < _preferredMinDistance)
        {
            Vector2 away = (Vector2)(_ctx.Transform.position - target.position);
            if (away.sqrMagnitude > 0.0001f)
            {
                _ctx.Movement.SetMoveInput(away.normalized);
            }
        }
        else
        {
            _ctx.Movement.Stop();
        }

        if (_ranged != null && _ranged.TryAttack(target))
        {
            _anim?.TriggerAttack();
        }
    }
}
