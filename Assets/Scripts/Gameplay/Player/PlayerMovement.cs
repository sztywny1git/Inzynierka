using UnityEngine;

[RequireComponent(typeof(ActionConstraintSystem))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(IStatsProvider))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private StatDefinition moveSpeedStat;

    private IStatsProvider _stats;
    private Rigidbody2D _rb;
    private Animator _animator;
    private ActionConstraintSystem _constraintSystem;
    private Vector2 _moveInput;
    private float _stopDelayTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _animator = GetComponentInChildren<Animator>();
        _stats = GetComponent<IStatsProvider>();
        _constraintSystem = GetComponent<ActionConstraintSystem>();
    }

    private void Update()
    {
        UpdateAnimation();
        FlipSprite();
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
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (_animator == null) return;

        bool hasInput = _moveInput.sqrMagnitude > 0.01f;

        if (hasInput)
        {
            _stopDelayTimer = 0f;
            _animator.SetBool("IsWalking", true);
        }
        else
        {
            _stopDelayTimer += Time.deltaTime;

            if (_stopDelayTimer > 0.1f)
            {
                _animator.SetBool("IsWalking", false);
            }
        }
    }

    private void FlipSprite()
    {
        if (!_constraintSystem.CanMove) return;

        if (Mathf.Abs(_moveInput.x) > 0.01f)
        {
            float direction = Mathf.Sign(_moveInput.x);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
    }
}