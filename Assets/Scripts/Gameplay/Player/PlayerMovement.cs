using UnityEngine;
using UnityEngine.InputSystem;

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
        float finalMoveSpeed = _stats.GetFinalStatValue(moveSpeedStat);
        _rb.linearVelocity = _moveInput * finalMoveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            _moveInput = Vector2.zero;
        }
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

        bool isWalking = _moveInput.sqrMagnitude > 0.01f;
        _animator.SetBool("isWalking", isWalking);
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