using UnityEngine;
using VContainer.Unity;

public class InputService : IInputService, IInitializable, ITickable, System.IDisposable
{
    public Vector2 Movement { get; private set; }
    
    private readonly PlayerControls _controls;

    public InputService()
    {
        _controls = new PlayerControls();
    }

    public void Initialize()
    {
        _controls.Enable();
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