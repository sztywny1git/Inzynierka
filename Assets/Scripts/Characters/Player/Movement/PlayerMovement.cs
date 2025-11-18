using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private int lastFacing = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Update walking animation state
        bool isWalking = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("isWalking", isWalking);

        // Update facing direction based on horizontal input
        if (moveInput.x != 0)
            lastFacing = moveInput.x > 0 ? 1 : -1;

        // Flip sprite based on facing direction
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * lastFacing;
        transform.localScale = scale;

        // Update animator movement parameters
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        animator.SetFloat("LastInputX", lastFacing);
        animator.SetFloat("LastInputY", moveInput.y != 0 ? Mathf.Sign(moveInput.y) : 0);
    }

    void FixedUpdate()
    {
        // Apply movement velocity
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        // Update movement input from player
        moveInput = context.ReadValue<Vector2>();
        if (context.canceled) moveInput = Vector2.zero;
    }
}
