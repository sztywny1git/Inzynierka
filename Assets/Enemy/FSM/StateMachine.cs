using System;

public sealed class StateMachine
{
    private IState _current;

    public IState Current => _current;

    public void ChangeState(IState next)
    {
        if (next == null) throw new ArgumentNullException(nameof(next));
        if (ReferenceEquals(_current, next)) return;

        _current?.Exit();
        _current = next;
        _current.Enter();
    }

    public void Tick(float deltaTime)
    {
        _current?.Tick(deltaTime);
    }
}
