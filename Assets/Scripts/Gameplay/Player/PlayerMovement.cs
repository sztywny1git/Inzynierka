using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(IStatsProvider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Data Dependencies")]
    [SerializeField] private StatDefinition moveSpeedStat;

    private IStatsProvider _stats;
    private Rigidbody2D _rb;
    private Animator _animator;

    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _stats = GetComponent<IStatsProvider>();
    }

    private void Update()
    {
        UpdateAnimation();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        if (_stats == null || moveSpeedStat == null) return;
        
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
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (_animator == null) return;

        bool IsWalking = _moveInput.sqrMagnitude > 0.01f;
        _animator.SetBool("IsWalking", IsWalking);
    }

    private void FlipSprite()
    {
        if (Mathf.Abs(_moveInput.x) > 0.01f)
        {
            float direction = Mathf.Sign(_moveInput.x);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
        }
    }
}