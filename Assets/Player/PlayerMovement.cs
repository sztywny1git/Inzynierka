using UnityEngine;

[RequireComponent(typeof(ActionConstraintSystem))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(IStatsProvider))]
public class PlayerMovement : MonoBehaviour, IMovementProvider
{
    [SerializeField] private StatDefinition moveSpeedStat;

    private IStatsProvider _stats;
    private Rigidbody2D _rb;
    private ActionConstraintSystem _constraintSystem;
    private Vector2 _moveInput;

    public Vector2 MovementDirection => _moveInput;
    public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _stats = GetComponent<IStatsProvider>();
        _constraintSystem = GetComponent<ActionConstraintSystem>();
    }

    private void FixedUpdate()
    {
        if (_stats == null || moveSpeedStat == null) return;
        
        if (!_constraintSystem.CanMove)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float finalMoveSpeed = _stats.GetFinalStatValue(moveSpeedStat);
        _rb.linearVelocity = _moveInput * finalMoveSpeed;
    }

    public void SetMoveInput(Vector2 input)
    {
        _moveInput = input;
    }

    public void StopMovement()
    {
        _moveInput = Vector2.zero;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }
}