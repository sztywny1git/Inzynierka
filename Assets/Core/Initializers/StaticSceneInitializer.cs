using UnityEngine;
using VContainer;

public class StaticSceneInitializer : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _poolContainer;
    
    private PlayerSpawnManager _spawnManager;
    private IAbilitySpawner _abilitySpawner;

    [Inject]
    public void Construct(PlayerSpawnManager spawnManager, IAbilitySpawner abilitySpawner)
    {
        _spawnManager = spawnManager;
        _abilitySpawner = abilitySpawner;
    }

    private void Start()
    {
        if (_poolContainer == null) _poolContainer = transform;

        // POPRAWKA: Używamy SetPoolContainer zamiast InitializeForScene
        _abilitySpawner.SetPoolContainer(_poolContainer);

        if (_spawnPoint == null)
        {
            var go = new GameObject("DefaultSpawnPoint");
            _spawnPoint = go.transform;
        }
        
        _spawnManager.SpawnPlayer(_spawnPoint);
    }
}