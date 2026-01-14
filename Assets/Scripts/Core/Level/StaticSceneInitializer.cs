using UnityEngine;
using VContainer;

public class StaticSceneInitializer : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    
    private PlayerSpawnManager _spawnManager;

    [Inject]
    public void Construct(PlayerSpawnManager spawnManager)
    {
        _spawnManager = spawnManager;
    }

    private void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = new GameObject("DefaultSpawnPoint").transform;
        }
        
        _spawnManager.SpawnPlayer(spawnPoint);
    }
}