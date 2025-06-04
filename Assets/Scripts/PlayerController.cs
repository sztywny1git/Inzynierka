using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Idle Sprites")]
    [SerializeField] private Sprite idleUpSprite;
    [SerializeField] private Sprite idleDownSprite;
    [SerializeField] private Sprite idleLeftSprite;
    [SerializeField] private Sprite idleRightSprite;
    
    [Header("Walking Sprites 1")]
    [SerializeField] private Sprite walkUpSprite1;
    [SerializeField] private Sprite walkDownSprite1;
    [SerializeField] private Sprite walkLeftSprite1;
    [SerializeField] private Sprite walkRightSprite1;
    
    [Header("Walking Sprites 2")]
    [SerializeField] private Sprite walkUpSprite2;
    [SerializeField] private Sprite walkDownSprite2;
    [SerializeField] private Sprite walkLeftSprite2;
    [SerializeField] private Sprite walkRightSprite2;
    
    [Header("Animation Settings")]
    [SerializeField] private float walkAnimationSpeed = 0.2f;
    
    private SpriteRenderer spriteRenderer;
    private float walkTimer = 0f;
    private bool isWalking = false;
    private Vector2 lastMoveDirection = Vector2.down; // Domyślnie patrzy w dół
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float maxSpeed = 8f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            if (idleDownSprite != null) spriteRenderer.sprite = idleDownSprite;
        }
    }

    private void Update()
    {
        // Pobieranie wejścia z klawiatury (strzałki lub WASD)
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized; // Normalizacja, aby ruch po skosie nie był szybszy

        // Aktualizacja sprite'a w zależności od kierunku ruchu
        UpdateSprite();

        // Obracanie postaci jest teraz obsługiwane w UpdateSprite()
    }

    private void FixedUpdate()
    {
        // Obliczanie docelowej prędkości
        Vector2 targetVelocity = movementInput * moveSpeed;
        
        // Płynne przejście do docelowej prędkości
        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            targetVelocity,
            (movementInput.magnitude > 0.1f ? acceleration : deceleration) * Time.fixedDeltaTime
        );

        // Ograniczenie maksymalnej prędkości
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;
        
        // Aktualizacja timera animacji
        if (movementInput.magnitude > 0.1f)
        {
            isWalking = true;
            lastMoveDirection = movementInput.normalized;
            walkTimer += Time.deltaTime;
        }
        else
        {
            isWalking = false;
        }
        
        // Wybór odpowiedniego sprite'a w zależności od kierunku
        if (isWalking)
        {
            // Określenie dominującego kierunku ruchu
            if (Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y))
            {
                // Ruch w poziomie
                if (movementInput.x > 0)
                {
                    // W prawo
                    transform.localScale = new Vector3(1, 1, 1);
                    SetWalkingSprite(walkRightSprite1, walkRightSprite2);
                }
                else
                {
                    // W lewo
                    transform.localScale = new Vector3(1, 1, 1);
                    SetWalkingSprite(walkLeftSprite1, walkLeftSprite2);
                }
            }
            else
            {
                // Ruch w pionie
                transform.localScale = new Vector3(1, 1, 1);
                
                if (movementInput.y > 0)
                {
                    // W górę
                    SetWalkingSprite(walkUpSprite1, walkUpSprite2);
                }
                else if (movementInput.y < 0)
                {
                    // W dół
                    SetWalkingSprite(walkDownSprite1, walkDownSprite2);
                }
            }
        }
        else
        {
            // Postać stoi - użyj odpowiedniego sprite'a spoczynkowego
            if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y))
            {
                // Ostatni ruch był w poziomie
                if (lastMoveDirection.x > 0)
                {
                    if (idleRightSprite != null) spriteRenderer.sprite = idleRightSprite;
                }
                else
                {
                    if (idleLeftSprite != null) spriteRenderer.sprite = idleLeftSprite;
                }
            }
            else
            {
                // Ostatni ruch był w pionie
                if (lastMoveDirection.y > 0)
                {
                    if (idleUpSprite != null) spriteRenderer.sprite = idleUpSprite;
                }
                else
                {
                    if (idleDownSprite != null) spriteRenderer.sprite = idleDownSprite;
                }
            }
        }
    }
    
    private void SetWalkingSprite(Sprite sprite1, Sprite sprite2)
    {
        if (sprite1 == null || sprite2 == null) return;
        
        // Przełączanie między dwoma klatkami animacji
        bool useFirstFrame = (Mathf.FloorToInt(walkTimer / walkAnimationSpeed) % 2) == 0;
        spriteRenderer.sprite = useFirstFrame ? sprite1 : sprite2;
    }
}
