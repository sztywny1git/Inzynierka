using UnityEngine;

public sealed class EnemyIdleState : IState
{
    private readonly EnemyContext _ctx;
    private readonly StateMachine _fsm;
    private readonly float _idleSeconds;
    private readonly IState _next;

    private float _timeLeft;

    public EnemyIdleState(EnemyContext ctx, StateMachine fsm, float idleSeconds, IState next)
    {
        _ctx = ctx;
        _fsm = fsm;
        _idleSeconds = Mathf.Max(0f, idleSeconds);
        _next = next;
    }

    public void Enter()
    {
        _timeLeft = _idleSeconds;
        _ctx.Movement.Stop();
    }

    public void Exit() { }

    public void Tick(float deltaTime)
    {
        if (_ctx.Brain.TryGetDesiredCombatState(_fsm.Current, out var desired))
        {
            _fsm.ChangeState(desired);
            return;
        }

        _timeLeft -= deltaTime;
        if (_timeLeft <= 0f)
        {
            _fsm.ChangeState(_next);
        }
    }
}
