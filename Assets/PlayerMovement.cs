using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private bool hasDirectionBeenSet = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (moveInput != Vector2.zero)
        {
            animator.SetBool("isWalking", true);

            // Zapamiêtujemy kierunek tylko raz — po pierwszym ruchu
            if (!hasDirectionBeenSet)
            {
                if (moveInput.x != 0)
                {
                    animator.SetFloat("LastInputX", Mathf.Sign(moveInput.x));
                    animator.SetFloat("LastInputY", 0);
                }
                else if (moveInput.y != 0)
                {
                    animator.SetFloat("LastInputX", 0);
                    animator.SetFloat("LastInputY", Mathf.Sign(moveInput.y));
                }

                hasDirectionBeenSet = true;
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
            hasDirectionBeenSet = false; // Reset przy puszczeniu klawiszy
        }
    }

}

