using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using System;

public class InputService : IInputService, IInitializable, ITickable, IDisposable
{
    public Vector2 Movement { get; private set; }
    
    public event Action OnPausePressed;
    public event Action<InputAction.CallbackContext> OnAttackPerformed;
    public event Action<InputAction.CallbackContext> OnSkillPerformed;
    public event Action OnInteractPressed;
    private readonly PlayerControls _controls;

    public InputService()
    {
        _controls = new PlayerControls();
    }

    public void Initialize()
    {
        _controls.Enable();
        
        _controls.Player.Pause.performed += ctx => OnPausePressed?.Invoke();
        _controls.Player.Attack.performed += ctx => OnAttackPerformed?.Invoke(ctx);
        _controls.Player.Skill.performed += ctx => OnSkillPerformed?.Invoke(ctx);
        _controls.Player.Interact.performed += ctx => OnInteractPressed?.Invoke();
    }

    public void Tick()
    {
        Movement = _controls.Player.Move.ReadValue<Vector2>();
    }

    public void Dispose()
    {
        _controls.Disable();
    }
}