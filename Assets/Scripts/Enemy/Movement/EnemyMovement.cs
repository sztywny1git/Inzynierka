using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Data Dependencies")]
    [SerializeField] private StatDefinition moveSpeedStat;

    [Header("Fallbacks")]
    [SerializeField] private float defaultMoveSpeedIfStatMissingOrZero = 2.5f; // Slower than player (3)

    private IStatsProvider _stats;
    private Rigidbody2D _rb;

    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<IStatsProvider>();
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        _moveInput = moveInput;
        if (_moveInput.sqrMagnitude > 1f) _moveInput = _moveInput.normalized;
    }

    public void Stop()
    {
        _moveInput = Vector2.zero;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        float finalMoveSpeed = 0f;
        if (_stats != null && moveSpeedStat != null)
        {
            finalMoveSpeed = _stats.GetFinalStatValue(moveSpeedStat);
        }
        if (finalMoveSpeed <= 0f)
        {
            finalMoveSpeed = defaultMoveSpeedIfStatMissingOrZero;
        }

        _rb.linearVelocity = _moveInput * finalMoveSpeed;
    }
}
