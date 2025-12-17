using UnityEngine;
using VContainer;

public class LevelInitializer : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    private GameplayEventBus _eventBus;

    [Inject]
    public void Construct(GameplayEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private void Start()
    {
        _eventBus.PublishLevelReady(spawnPoint);
    }
}