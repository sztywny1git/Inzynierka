using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f; // Czas między strzałami w sekundach
    [SerializeField] private float bulletSpeed = 10f;
    
    private float nextFireTime = 0f;
    private PlayerController playerController;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        // Jeśli na tym samym obiekcie jest PlayerController, to PlayerController obsługuje strzelanie.
        // Wyłącz ten komponent, aby uniknąć podwójnego strzelania i konfliktów wejścia.
        if (playerController != null)
        {
            Debug.Log("PlayerShooting: wyłączony, ponieważ PlayerController obsługuje strzelanie.");
            enabled = false;
            return;
        }
        
        // Jeśli nie ma firePoint, utwórz go
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.right * 0.5f; // Pozycja przed graczem
            firePoint = firePointObj.transform;
        }
    }
    
    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab nie jest przypisany!");
            return;
        }
        
        // Pobierz kierunek strzału z gracza
        Vector2 shootDirection = GetShootDirection();
        
        // Utwórz pocisk
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDirection);
        }
        else
        {
            // Jeśli nie ma skryptu Bullet, dodaj podstawowy ruch
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = shootDirection * bulletSpeed;
            }
        }
    }
    
    private Vector2 GetShootDirection()
    {
        // Użyj kierunku myszy jeśli jest dostępny
        Vector2 mousePosition = Camera.main != null
            ? (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition)
            : (Vector2)Input.mousePosition;
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;
        
        // Jeśli mysz jest zbyt blisko gracza, użyj ostatniego kierunku ruchu
        if (direction.magnitude < 0.1f)
        {
            // Pobierz ostatni kierunek ruchu z PlayerController
            // Można dodać publiczną właściwość w PlayerController do tego
            direction = Vector2.right; // Domyślny kierunek
        }
        
        return direction;
    }
    
    private void Update()
    {
        // Strzelanie LPM (klik i przytrzymanie) z wykorzystaniem starego Input API
        bool pressed = Input.GetMouseButtonDown(0);
        bool held = Input.GetMouseButton(0);

        if ((pressed || held) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Aktualizuj pozycję firePoint w zależności od kierunku do myszy
        UpdateFirePointPosition();
    }
    
    private void UpdateFirePointPosition()
    {
        if (firePoint == null) return;
        Vector2 mousePosition = Camera.main != null
            ? (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition)
            : (Vector2)Input.mousePosition;
        Vector2 dir = (mousePosition - (Vector2)transform.position);
        if (dir.sqrMagnitude > 0.0001f)
        {
            firePoint.localPosition = dir.normalized * 0.5f;
        }
    }
}
