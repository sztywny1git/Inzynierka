using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Parameters")]
    [SerializeField] private string isWalkingBool = "isWalking";
    [SerializeField] private string hitTrigger = "hit";
    [SerializeField] private string attackTrigger = "attack";
    [SerializeField] private string dieTrigger = "die";

    [Header("Walking")]
    [SerializeField] private float walkingVelocityThreshold = 0.05f;

    private Rigidbody2D _rb;

    private int _isWalkingHash;
    private int _hitHash;
    private int _attackHash;
    private int _dieHash;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        _isWalkingHash = Animator.StringToHash(isWalkingBool);
        _hitHash = Animator.StringToHash(hitTrigger);
        _attackHash = Animator.StringToHash(attackTrigger);
        _dieHash = Animator.StringToHash(dieTrigger);
    }

    private void Update()
    {
        if (animator == null || _rb == null) return;

        bool isWalking = _rb.linearVelocity.sqrMagnitude > walkingVelocityThreshold * walkingVelocityThreshold;
        animator.SetBool(_isWalkingHash, isWalking);
    }

    public void TriggerHit()
    {
        if (animator == null) return;
        animator.SetTrigger(_hitHash);
    }

    public void TriggerAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(_attackHash);
    }

    public void TriggerDie()
    {
        if (animator == null) return;
        animator.SetTrigger(_dieHash);
    }
}
