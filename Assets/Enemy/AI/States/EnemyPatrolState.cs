using UnityEngine;

public sealed class EnemyPatrolState : IState
{
    private readonly EnemyContext _ctx;
    private readonly StateMachine _fsm;
    private readonly IPatrolPointProvider _patrolProvider;
    private readonly float _arriveDistance;
    private readonly float _repathSeconds;

    private Vector3 _currentTarget;
    private float _repathTimer;

    public EnemyPatrolState(
        EnemyContext ctx,
        StateMachine fsm,
        IPatrolPointProvider patrolProvider,
        float arriveDistance,
        float repathSeconds)
    {
        _ctx = ctx;
        _fsm = fsm;
        _patrolProvider = patrolProvider;
        _arriveDistance = Mathf.Max(0.05f, arriveDistance);
        _repathSeconds = Mathf.Max(0.1f, repathSeconds);
    }

    public void Enter()
    {
        _repathTimer = 0f;
        PickNewPoint();
    }

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

        _repathTimer -= deltaTime;

        Vector2 to = (Vector2)(_currentTarget - _ctx.Transform.position);
        float dist = to.magnitude;

        if (dist <= _arriveDistance || _repathTimer <= 0f)
        {
            PickNewPoint();
            to = (Vector2)(_currentTarget - _ctx.Transform.position);
        }

        _ctx.Movement.SetMoveInput(to.normalized);
    }

    private void PickNewPoint()
    {
        _repathTimer = _repathSeconds;

        if (_patrolProvider != null && _patrolProvider.TryGetNextPoint(_ctx.Transform.position, out var p))
        {
            _currentTarget = p;
        }
        else
        {
            // Fallback: small random step
            Vector2 random = Random.insideUnitCircle.normalized;
            _currentTarget = _ctx.Transform.position + (Vector3)(random * 2f);
        }

        _currentTarget.z = _ctx.Transform.position.z;
    }
}
