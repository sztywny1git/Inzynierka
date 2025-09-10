using UnityEngine;
using UnityEngine.EventSystems;

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

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint; // Opcjonalny punkt strzału; jeśli null, użyje pozycji gracza
    [SerializeField] private float fireRate = 8f; // pocisków na sekundę przy przytrzymaniu LPM
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
        // Pobieranie wejścia z klawiatury (strzałki lub WASD)
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized; // Normalizacja, aby ruch po skosie nie był szybszy

        // Aktualizacja sprite'a w zależności od kierunku ruchu
        UpdateSprite();

        // Obracanie postaci jest teraz obsługiwane w UpdateSprite()

        // Strzelanie spacją (klik lub przytrzymanie)
        HandleShooting();
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

    private void HandleShooting()
    {
        // Strzelanie spacją
        bool pressed = Input.GetKeyDown(KeyCode.Space);
        bool held = Input.GetKey(KeyCode.Space);

        if (!pressed && !held)
            return;

        // Ograniczenie szybkostrzelności przy przytrzymaniu
        if (held && Time.time < nextFireTime)
            return;

        if (bulletPrefab == null)
        {
            Debug.LogWarning("Brak przypisanego prefab’u Bullet w PlayerController");
            return;
        }

        Vector3 firePos = firePoint != null ? firePoint.position : transform.position;
        // Kierunek strzału = aktualny kierunek postaci (4 kierunki)
        Vector2 dir = GetFacingDirection4();
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.down; // domyślnie w dół, jak przy starcie

        GameObject bulletObj = Instantiate(bulletPrefab, firePos, Quaternion.identity);
        var bulletComp = bulletObj.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.SetDirection(dir);
        }
        else
        {
            var rb2d = bulletObj.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.linearVelocity = dir * 10f; // domyślna prędkość, jeśli brak skryptu Bullet
            }
        }

        if (held)
        {
            nextFireTime = Time.time + (1f / Mathf.Max(0.01f, fireRate));
        }
    }

    // Zwraca aktualny kierunek patrzenia jako 4-kierunkowy wektor (prawo/lewo/góra/dół)
    private Vector2 GetFacingDirection4()
    {
        Vector2 dir = lastMoveDirection;
        if (dir.sqrMagnitude < 0.0001f)
            return Vector2.down; // domyślny

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x >= 0f ? Vector2.right : Vector2.left;
        }
        else
        {
            return dir.y >= 0f ? Vector2.up : Vector2.down;
        }
    }
}
