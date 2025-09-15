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

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletOffset = 1f; // Offset od gracza

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMoveDirection = Vector2.down;
    private float walkTimer = 0f;
    private bool isWalking = false;
    private float nextFireTime = 0f;

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
        if (StatsManager.Instance == null) return;

        // Pobranie wejścia
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Aktualizacja sprite'a
        UpdateSprite();

        // Obsługa strzelania
        HandleShooting();
    }

    private void FixedUpdate()
    {
        if (StatsManager.Instance == null) return;

        float moveSpeed = StatsManager.Instance.moveSpeed;
        float acceleration = StatsManager.Instance.acceleration;
        float deceleration = StatsManager.Instance.deceleration;
        float maxSpeed = StatsManager.Instance.maxSpeed;

        // Ruch
        Vector2 targetVelocity = movementInput * moveSpeed;
        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            targetVelocity,
            (movementInput.magnitude > 0.1f ? acceleration : deceleration) * Time.fixedDeltaTime
        );

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

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

        if (isWalking)
        {
            if (Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y))
            {
                // Poziomo
                if (movementInput.x > 0) SetWalkingSprite(walkRightSprite1, walkRightSprite2);
                else SetWalkingSprite(walkLeftSprite1, walkLeftSprite2);
            }
            else
            {
                // Pionowo
                if (movementInput.y > 0) SetWalkingSprite(walkUpSprite1, walkUpSprite2);
                else SetWalkingSprite(walkDownSprite1, walkDownSprite2);
            }
        }
        else
        {
            // Idle
            if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y))
            {
                if (lastMoveDirection.x > 0) spriteRenderer.sprite = idleRightSprite;
                else spriteRenderer.sprite = idleLeftSprite;
            }
            else
            {
                if (lastMoveDirection.y > 0) spriteRenderer.sprite = idleUpSprite;
                else spriteRenderer.sprite = idleDownSprite;
            }
        }
    }

    private void SetWalkingSprite(Sprite sprite1, Sprite sprite2)
    {
        if (sprite1 == null || sprite2 == null) return;
        bool useFirstFrame = (Mathf.FloorToInt(walkTimer / walkAnimationSpeed) % 2) == 0;
        spriteRenderer.sprite = useFirstFrame ? sprite1 : sprite2;
    }

    private void HandleShooting()
    {
        if (StatsManager.Instance == null) return;

        float fireRate = StatsManager.Instance.fireRate;
        bool pressed = Input.GetKeyDown(KeyCode.Space);
        bool held = Input.GetKey(KeyCode.Space);

        if (!pressed && !held) return;
        if (held && Time.time < nextFireTime) return;

        if (bulletPrefab == null)
        {
            Debug.LogWarning("Brak prefab'u Bullet w PlayerController");
            return;
        }

        Vector2 dir = GetFacingDirection4();
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.down;

        // Oblicz pozycję spawnu bulleta z offsetem
        Vector3 firePos;
        if (firePoint != null)
        {
            firePos = firePoint.position;
        }
        else
        {
            // Spawn bulleta z offsetem w kierunku patrzenia
            firePos = transform.position + (Vector3)(dir * bulletOffset);
        }

        // Debug informacje
        Debug.Log($"Spawning bullet at: {firePos}, direction: {dir}");

        GameObject bulletObj = Instantiate(bulletPrefab, firePos, Quaternion.identity);
        var bulletComp = bulletObj.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.SetDirection(dir);
        }
        else
        {
            Debug.LogWarning("Bullet prefab nie ma komponentu Bullet!");
            var rb2d = bulletObj.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.linearVelocity = dir * StatsManager.Instance.bulletSpeed;
            }
        }

        if (held) nextFireTime = Time.time + (1f / Mathf.Max(0.01f, fireRate));
    }

    private Vector2 GetFacingDirection4()
    {
        Vector2 dir = lastMoveDirection;
        if (dir.sqrMagnitude < 0.0001f) return Vector2.down;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x >= 0f ? Vector2.right : Vector2.left;
        else
            return dir.y >= 0f ? Vector2.up : Vector2.down;
    }
}
