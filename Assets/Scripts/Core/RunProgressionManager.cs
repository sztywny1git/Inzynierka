using System;
using UnityEngine;

public class RunProgressionManager : IDisposable
{
    private readonly GameplayEventBus _gameplayEvents;
    private readonly SessionData _sessionState;

    public RunProgressionManager(GameplayEventBus gameplayEvents, SessionData sessionState)
    {
        _gameplayEvents = gameplayEvents;
        _sessionState = sessionState;
        
        _gameplayEvents.AdvanceToNextLevelRequested += AdvanceToNextLevel;
    }

    public void Dispose()
    {
        _gameplayEvents.AdvanceToNextLevelRequested -= AdvanceToNextLevel;
    }

    public void StartRun(RunDefinition runDefinition)
    {
        _sessionState.CurrentRun = runDefinition;
        _sessionState.CurrentLevelIndex = -1;
        AdvanceToNextLevel();
    }

    private void AdvanceToNextLevel()
    {
        _sessionState.CurrentLevelIndex++;
        
        var run = _sessionState.CurrentRun;
        if (run != null && run.LevelSequence.Count > 0)
        {
            int index = Mathf.Clamp(_sessionState.CurrentLevelIndex, 0, run.LevelSequence.Count - 1);
            var nextLevelDef = run.LevelSequence[index];

            _gameplayEvents.RequestLoadLevel(nextLevelDef);
        }
    }
}