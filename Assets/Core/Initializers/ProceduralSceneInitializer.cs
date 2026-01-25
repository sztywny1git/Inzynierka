using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

public class ProceduralSceneInitializer : MonoBehaviour
{
    [SerializeField] private RoomFirstDungeonGenerator _dungeonGenerator;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private Transform _poolContainer;

    private PlayerSpawnManager _spawnManager;
    private IAbilitySpawner _abilitySpawner;
    private IObjectResolver _container;
    private LootSystem _lootSystem;
    private GameplayEventBus _gameplayEvents;

    [Inject]
    public void Construct(
        PlayerSpawnManager spawnManager,
        IAbilitySpawner abilitySpawner,
        IObjectResolver container,
        LootSystem lootSystem,
        GameplayEventBus gameplayEvents)
    {
        _spawnManager = spawnManager;
        _abilitySpawner = abilitySpawner;
        _container = container;
        _lootSystem = lootSystem;
        _gameplayEvents = gameplayEvents;
    }

    private void Awake()
    {
        if (_poolContainer == null) _poolContainer = transform;
    }

    public async UniTask GenerateLevel()
    {
        GameObject lootHolder = new GameObject("LootHolder");
        lootHolder.transform.SetParent(transform);
        _lootSystem.SetLootContainer(lootHolder.transform);

        if (_enemySpawner != null)
        {
            _container.Inject(_enemySpawner);
        }

        _abilitySpawner.SetPoolContainer(_poolContainer);

        _dungeonGenerator.GenerateDungeon();
        Physics2D.SyncTransforms();

        if (_enemySpawner != null)
        {
            _enemySpawner.SpawnEnemies();

            Vector2Int bossCoords = _dungeonGenerator.GetBossSpawnPosition();
            _enemySpawner.SpawnBoss(bossCoords);
        }

        Vector2Int startCoords = _dungeonGenerator.GetPlayerSpawnPosition();
        var spawnGO = new GameObject("PlayerSpawnPoint");
        spawnGO.transform.position = new Vector3(startCoords.x, startCoords.y, 0);
        
        _spawnManager.SpawnPlayer(spawnGO.transform);

        Physics2D.SyncTransforms();
        await UniTask.WaitForEndOfFrame(this);

        _gameplayEvents.InvokeLevelReady(spawnGO.transform);
    }
}