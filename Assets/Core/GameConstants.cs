using UnityEngine;

[CreateAssetMenu(fileName = "GameConstants", menuName = "Core/Game Constants")]
public class GameConstants : ScriptableObject
{
    [Header("Registries")]
    [SerializeField] private PlayerRegistry _playerRegistry;
    public PlayerRegistry PlayerRegistry => _playerRegistry;

    [SerializeField] private EnemyRegistry _enemyRegistry;
    public EnemyRegistry EnemyRegistry => _enemyRegistry;

    [Header("Player Defaults")]
    [SerializeField] private CharacterDefinition _defaultPlayerClass;
    public CharacterDefinition DefaultPlayerClass => _defaultPlayerClass;

    [Header("Run Settings")]
    [SerializeField] private int _maxLevels = 5;
    public int MaxLevels => _maxLevels;

    [Header("Prefabs")]
    public Loot lootPrefab;

}