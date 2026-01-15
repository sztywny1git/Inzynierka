/// <summary>
/// Interface for hierarchical state machine states.
/// Extends IState with support for sub-states.
/// </summary>
public interface IHierarchicalState : IState
{
    /// <summary>
    /// The currently active sub-state, if any.
    /// </summary>
    IState CurrentSubState { get; }
    
    /// <summary>
    /// Whether this state has active sub-states.
    /// </summary>
    bool HasSubStates { get; }
    
    /// <summary>
    /// Called when a sub-state requests a transition.
    /// </summary>
    void OnSubStateComplete();
}
