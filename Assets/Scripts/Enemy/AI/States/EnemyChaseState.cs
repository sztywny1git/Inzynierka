using UnityEngine;

public sealed class EnemyChaseState : IState
{
    private readonly EnemyContext _ctx;
    private readonly StateMachine _fsm;
    private readonly float _giveUpDistance;
    private readonly IState _fallback;

    public EnemyChaseState(EnemyContext ctx, StateMachine fsm, float giveUpDistance, IState fallback)
    {
        _ctx = ctx;
        _fsm = fsm;
        _giveUpDistance = Mathf.Max(0.1f, giveUpDistance);
        _fallback = fallback;
    }

    public void Enter() { }

    public void Exit()
    {
        _ctx.Movement.Stop();
    }

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

        Vector2 to = (Vector2)(target.position - _ctx.Transform.position);
        if (to.magnitude > _giveUpDistance)
        {
            _fsm.ChangeState(_fallback);
            return;
        }

        _ctx.Movement.SetMoveInput(to.normalized);
    }
}
