using UnityEngine;
using VContainer;

public class ProceduralSceneInitializer : MonoBehaviour
{
    private SessionData _sessionData;
    private DungeonGenerator _dungeonGenerator;
    private LevelBuilder _levelBuilder;
    private PlayerSpawnManager _spawnManager;

    [Inject]
    public void Construct(
        SessionData sessionData,
        DungeonGenerator dungeonGenerator,
        LevelBuilder levelBuilder,
        PlayerSpawnManager spawnManager)
    {
        _sessionData = sessionData;
        _dungeonGenerator = dungeonGenerator;
        _levelBuilder = levelBuilder;
        _spawnManager = spawnManager;
    }

    private void Start()
    {
        var levelDef = _sessionData.CurrentLevelDefinition;
        if (levelDef == null)
        {
            Debug.LogError("ProceduralSceneInitializer: Missing LevelDefinition!");
            return;
        }

        var levelData = _dungeonGenerator.Generate(levelDef, Random.Range(0, 99999));
        _levelBuilder.Build(levelData);

        var spawnGO = new GameObject("ProceduralSpawnPoint");
        spawnGO.transform.position = levelData.PlayerSpawnPosition;
        
        _spawnManager.SpawnPlayer(spawnGO.transform);
    }
}