using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerController : MonoBehaviour
{
    private Character _character;
    private PlayerMovement _playerMovement;
    private Camera _mainCamera;
    private Interactor _interactor;
    private IInputService _inputService;
    private GameplayEventBus _eventbus;

    [Inject]
    public void Construct(IInputService inputService, GameplayEventBus eventbus)
    {
        _inputService = inputService;
        _eventbus = eventbus;
    }

    private void Awake()
    {
        _character = GetComponent<Character>();
        _playerMovement = GetComponent<PlayerMovement>();
        _interactor = GetComponent<Interactor>();
        _mainCamera = Camera.main;

        if (_inputService != null)
        {
            _inputService.OnAttackPerformed += OnAttack;
            _inputService.OnSkillPerformed += OnSkill;
            _inputService.OnInteractPressed += OnInteract;
        }

        if (_character != null && _character.Health != null)
        {
            _character.Health.Death += OnDeath;
        }
    }

    private void OnDestroy()
    {
        if (_inputService != null)
        {
            _inputService.OnAttackPerformed -= OnAttack;
            _inputService.OnSkillPerformed -= OnSkill;
            _inputService.OnInteractPressed -= OnInteract;
        }
        
        if (_character != null && _character.Health != null)
        {
            _character.Health.Death -= OnDeath;
        }
    }


    private void Update()
    {
        _playerMovement.SetMoveInput(_inputService.Movement);
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        Vector3 aimPosition = GetAimPosition(); 
        _character.AbilityCaster.RequestAbility(0, aimPosition);
    }

    private void OnSkill(InputAction.CallbackContext context)
    {
        Vector3 aimPosition = GetAimPosition();
        _character.AbilityCaster.RequestAbility(1, aimPosition);
    }

    private void OnInteract()
    {
        _interactor?.PerformInteraction();
    }

    private void OnDeath()
    {
        _eventbus.InvokePlayerDied();
    }

    private Vector3 GetAimPosition()
{
    if (_mainCamera == null) _mainCamera = Camera.main;
    if (_mainCamera == null) return transform.position + transform.right;

    Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
    Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
    mouseWorldPos.z = 0;

    return mouseWorldPos;
}
}