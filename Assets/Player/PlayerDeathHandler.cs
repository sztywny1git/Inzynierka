using UnityEngine;
using VContainer;

public class PlayerDeathHandler : BaseDeathHandler
{
    private PlayerController _playerController;
    private AbilityCaster _abilityCaster;
    private CharacterVisuals _characterVisuals;
    private GameplayEventBus _gameplayEvents;

    [Inject]
    public void Construct(GameplayEventBus gameplayEvents)
    {
        _gameplayEvents = gameplayEvents;
    }

    protected override void Awake()
    {
        base.Awake();
        _playerController = GetComponent<PlayerController>();
        _abilityCaster = GetComponent<AbilityCaster>();
        _characterVisuals = GetComponentInChildren<CharacterVisuals>();
    }

    protected override void HandleDeath()
    {
        if (_abilityCaster != null)
        {
            _abilityCaster.enabled = false;
        }

        if (_characterVisuals != null)
        {
            _characterVisuals.ResetAttackTriggers();
        }

        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        base.HandleDeath();
        _gameplayEvents.InvokePlayerDied();
    }
}