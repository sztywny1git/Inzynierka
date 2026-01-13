using System;
using UnityEngine;

/// <summary>
/// State that handles the transition animation/behavior between boss phases.
/// </summary>
public class BossPhaseTransitionState : IState
{
    private readonly BossContext _context;
    private float _transitionDuration;
    private int _targetPhase;
    private float _elapsedTime;
    
    public event Action OnTransitionComplete;
    
    public BossPhaseTransitionState(BossContext context, float duration, int targetPhase)
    {
        _context = context;
        _transitionDuration = duration;
        _targetPhase = targetPhase;
    }
    
    public void SetTargetPhase(int targetPhase, float duration)
    {
        _targetPhase = targetPhase;
        _transitionDuration = duration;
    }
    
    public void Enter()
    {
        _elapsedTime = 0f;
        
        // Boss could play a roar, power-up animation, etc.
        _context.Animator?.PlayPhaseTransition();
    }
    
    public void Exit()
    {
        // Clean up transition effects if any
    }
    
    public void Tick(float deltaTime)
    {
        _elapsedTime += deltaTime;
        
        if (_elapsedTime >= _transitionDuration)
        {
            OnTransitionComplete?.Invoke();
        }
    }
}
