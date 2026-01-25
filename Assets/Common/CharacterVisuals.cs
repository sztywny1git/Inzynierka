using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterVisuals : MonoBehaviour, IFacingHandler
{
    [SerializeField] private string _speedParameterName = "AttackSpeed";
    [SerializeField] private string _walkingParameterName = "IsWalking";
    
    private Animator _animator;
    private ICastAnimationHandler _castHandler;
    private BaseDeathHandler _deathHandler;
    private IMovementProvider _movementProvider;
    private Rigidbody2D _rb;

    private Transform _rootTransform; 

    private string _lastTriggerName;
    private float _stopDelayTimer;
    private bool _isRotationLocked;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
        _castHandler = GetComponentInParent<ICastAnimationHandler>();
        _deathHandler = GetComponentInParent<BaseDeathHandler>();
        _movementProvider = GetComponentInParent<IMovementProvider>();
        _rb = GetComponentInParent<Rigidbody2D>();

        if (_rb != null)
        {
            _rootTransform = _rb.transform;
        }
        else
        {
            _rootTransform = transform.parent != null ? transform.parent : transform;
        }
    }

    private void OnEnable()
    {
        if (_castHandler != null)
        {
            _castHandler.OnCastAnimationRequired += PlayCastAnimation;
            _castHandler.OnCastInterrupted += OnCastInterrupted;
        }
    }

    private void OnDisable()
    {
        if (_castHandler != null)
        {
            _castHandler.OnCastAnimationRequired -= PlayCastAnimation;
            _castHandler.OnCastInterrupted -= OnCastInterrupted;
        }
    }

    private void Update()
    {
        HandleMovementAnimation();
        HandleSpriteFlip();
    }

    private void HandleMovementAnimation()
    {
        bool isMoving = false;

        if (_movementProvider != null)
        {
            isMoving = _movementProvider.IsMoving;
        }
        else if (_rb != null)
        {
            isMoving = _rb.linearVelocity.sqrMagnitude > 0.01f; 
        }

        if (isMoving)
        {
            _stopDelayTimer = 0f;
            _animator.SetBool(_walkingParameterName, true);
        }
        else
        {
            _stopDelayTimer += Time.deltaTime;

            if (_stopDelayTimer > 0.1f)
            {
                _animator.SetBool(_walkingParameterName, false);
            }
        }
    }

    private void HandleSpriteFlip()
    {
        if (_isRotationLocked) return;

        float xDirection = 0f;

        if (_movementProvider != null)
        {
            xDirection = _movementProvider.MovementDirection.x;
        }
        else if (_rb != null)
        {
            xDirection = _rb.linearVelocity.x;
        }

        PerformFlip(xDirection);
    }

    public void FaceDirection(Vector2 direction)
    {
        PerformFlip(direction.x);
    }

    private void PerformFlip(float xDirection)
    {
        if (Mathf.Abs(xDirection) > 0.01f)
        {
            float direction = Mathf.Sign(xDirection);
            
            Vector3 scale = _rootTransform.localScale;
            
            if (Mathf.Sign(scale.x) != direction)
            {
                scale.x = Mathf.Abs(scale.x) * direction;
                _rootTransform.localScale = scale;
            }
        }
    }

    private void PlayCastAnimation(string triggerName, float speedMultiplier)
    {
        if (!string.IsNullOrEmpty(triggerName))
        {
            _isRotationLocked = true;
            
            _lastTriggerName = triggerName;
            _animator.ResetTrigger("Interrupted");
            _animator.SetFloat(_speedParameterName, speedMultiplier);
            _animator.SetTrigger(triggerName);
        }
    }

    private void OnCastInterrupted()
    {
        _isRotationLocked = false;

        if (!string.IsNullOrEmpty(_lastTriggerName))
        {
            _animator.ResetTrigger(_lastTriggerName);
        }
        _animator.SetTrigger("Interrupted");
    }

    public void ResetAttackTriggers()
    {
        if (!string.IsNullOrEmpty(_lastTriggerName))
        {
            _animator.ResetTrigger(_lastTriggerName);
        }
        _animator.ResetTrigger("Interrupted");
    }

    public void OnAnimAttackPoint()
    {
        _isRotationLocked = false;
        if (_castHandler != null) _castHandler.OnAnimAttackPoint();
    }

    public void OnAnimFinish()
    {
        _isRotationLocked = false;
        if (_castHandler != null) _castHandler.OnAnimFinish();
    }

    public void OnDeathAnimationFinished()
    {
        if (_deathHandler != null) _deathHandler.OnDeathAnimationFinished();
    }
}