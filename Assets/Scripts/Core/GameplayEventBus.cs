using System;
using UnityEngine;

public class GameplayEventBus
{
    public event Action<Character> OnPlayerSpawned;
    public void PublishPlayerSpawned(Character player) => OnPlayerSpawned?.Invoke(player);

    public event Action<Transform> OnLevelReady;
    public void PublishLevelReady(Transform spawnPoint) => OnLevelReady?.Invoke(spawnPoint);

    public event Action<CharacterDefinition> OnClassSelected;
    public void PublishClassSelected(CharacterDefinition classDef) => OnClassSelected?.Invoke(classDef);

    public event Action<Character> OnCharacterDied;
    public void PublishCharacterDied(Character character) => OnCharacterDied?.Invoke(character);
}