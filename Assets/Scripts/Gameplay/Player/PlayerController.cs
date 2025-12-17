using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private AbilityCaster _abilityCaster;
    private PlayerMovement _playerMovement;
    private Camera _mainCamera;

    private void Awake()
    {
        _abilityCaster = GetComponent<AbilityCaster>();
        _playerMovement = GetComponent<PlayerMovement>();
        _mainCamera = Camera.main;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _playerMovement.Move(context);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 aimDirection = GetAimDirection();
            _abilityCaster.RequestAbility(0, aimDirection);
        }
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 aimDirection = GetAimDirection();
            _abilityCaster.RequestAbility(1, aimDirection);
        }
    }

    private Vector2 GetAimDirection()
    {
        if (_mainCamera == null) return transform.right;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
        return (mouseWorldPos - transform.position).normalized;
    }
}