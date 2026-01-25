using UnityEngine;
using UnityEngine.InputSystem;
using System;

public interface IInputService
{
    Vector2 Movement { get; }
    
    event Action OnPausePressed;
    event Action<InputAction.CallbackContext> OnAttackPerformed;
    event Action<InputAction.CallbackContext> OnSkillPerformed;
    event Action OnInteractPressed;
}
