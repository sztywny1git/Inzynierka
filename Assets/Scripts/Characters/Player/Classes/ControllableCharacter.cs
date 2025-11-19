// Plik: ControllableCharacter.cs
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerMovement))]
public class ControllableCharacter : MonoBehaviour
{
    public ClassData characterClassData;

    private PlayerAttack playerAttack;
    private PlayerMovement playerMovement;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerMovement = GetComponent<PlayerMovement>();
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    public void EnablePlayerControl()
    {
        playerAttack.enabled = true;
        playerMovement.enabled = true;
        this.gameObject.tag = "Player";
        
        if (playerInput != null)
        {
            InputAction moveAction = playerInput.actions.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed += OnMove;
                moveAction.canceled += OnMove;
            }
        }
    }

    public void DisablePlayerControl()
    {
        playerAttack.enabled = false;
        playerMovement.enabled = false;
        this.gameObject.tag = "NPC";
        
        if (playerInput != null)
        {
            InputAction moveAction = playerInput.actions.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed -= OnMove;
                moveAction.canceled -= OnMove;
            }
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (playerMovement.enabled)
        {
            playerMovement.Move(context);
        }
    }
}