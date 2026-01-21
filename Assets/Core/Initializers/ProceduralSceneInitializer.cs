using UnityEngine;
using VContainer;

public class ProceduralSceneInitializer : MonoBehaviour
{
    [SerializeField] private Transform _poolContainer;
    private PlayerSpawnManager _spawnManager;
    private IAbilitySpawner _abilitySpawner;

    [Inject]
    public void Construct(
        PlayerSpawnManager spawnManager,
        IAbilitySpawner abilitySpawner)
    {
        _spawnManager = spawnManager;
        _abilitySpawner = abilitySpawner;
    }

    private void Start()
    {
        if (_poolContainer == null) _poolContainer = transform;
        _abilitySpawner.SetPoolContainer(_poolContainer);

        var spawnGO = new GameObject("PlayerSpawnPoint");
        spawnGO.transform.position = Vector3.zero;
        
        _spawnManager.SpawnPlayer(spawnGO.transform);
    }
}