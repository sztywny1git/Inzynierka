using UnityEngine;
using VContainer;

public class ClassSelectorNPC : MonoBehaviour
{
    [SerializeField] private CharacterDefinition classToAssign;
    
    private GameplayEventBus _eventBus;
    private bool _playerCanInteract = false;

    [Inject]
    public void Construct(GameplayEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private void Update()
    {
        if (_playerCanInteract && Input.GetKeyDown(KeyCode.E))
        {
            _eventBus.PublishClassSelected(classToAssign);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerCanInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerCanInteract = false;
        }
    }
}