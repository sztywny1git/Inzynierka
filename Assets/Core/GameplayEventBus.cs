using System;
using UnityEngine;

public class GameplayEventBus
{
    public event Action<Character> PlayerSpawned;
    public void InvokePlayerSpawned(Character player) => PlayerSpawned?.Invoke(player);

    public event Action PlayerDied;
    public void InvokePlayerDied() => PlayerDied?.Invoke();

    public event Action RunWon;  
    public void InvokeRunWon() => RunWon?.Invoke();

    public event Action EndRunRequested;
    public void RequestEndRun() => EndRunRequested?.Invoke();

    public event Action GoToHubRequested;
    public void RequestGoToHub() => GoToHubRequested?.Invoke();

    public event Action<CharacterDefinition> ClassSelected;
    public void InvokeClassSelected(CharacterDefinition classDef) => ClassSelected?.Invoke(classDef);

    public event Action BeginRunRequested;
    public void RequestBeginRun() => BeginRunRequested?.Invoke();

    public event Action AdvanceToNextLevelRequested;
    public void RequestAdvanceToNextLevel() => AdvanceToNextLevelRequested?.Invoke();

    public event Action LevelLoadRequested;
    public void RequestLoadLevel() => LevelLoadRequested?.Invoke();

    public event Action<Transform> LevelReady;
    public void InvokeLevelReady(Transform spawnPoint) => LevelReady?.Invoke(spawnPoint);

    public event Action<Vector3, LootTableSO, int> OnEnemyDied;
    public void InvokeEnemyDied(Vector3 position, LootTableSO lootTable, int expAmount) => OnEnemyDied?.Invoke(position, lootTable, expAmount);

}