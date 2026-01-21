using System;

/// <summary>
/// A hierarchical finite state machine that supports nested states.
/// Each state can contain its own sub-state machine.
/// </summary>
public sealed class HierarchicalStateMachine
{
    private IState _currentState;
    
    public IState CurrentState => _currentState;
    
    public event Action<IState, IState> OnStateChanged;

    public void ChangeState(IState nextState)
    {
        if (nextState == null) throw new ArgumentNullException(nameof(nextState));
        if (ReferenceEquals(_currentState, nextState)) return;

        var previousState = _currentState;
        
        _currentState?.Exit();
        _currentState = nextState;
        _currentState.Enter();
        
        OnStateChanged?.Invoke(previousState, _currentState);
    }

    public void Tick(float deltaTime)
    {
        _currentState?.Tick(deltaTime);
    }
    
    public void Clear()
    {
        _currentState?.Exit();
        _currentState = null;
    }
}
