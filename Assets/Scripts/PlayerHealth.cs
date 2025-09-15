using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 6;      // maksymalne mo¿liwe HP
    public int startHealth = 3;    // pocz¹tkowe HP
    private int currentHealth;

    [Header("UI Hearts")]
    public GameObject[] hearts;    // przeci¹gnij w inspectorze wszystkie serca (6)

    [Header("Hit Settings")]
    public Transform hitbox;
    public float hitboxRadius;
    public LayerMask damageSource;
    private bool hitCooldown = false;

    [Header("Knockback & Effects")]
    public Rigidbody2D rb;
    public float knockBackForce = 10;
    public float knockBackForceUp = 2;
    public ParticleSystem hitParticle;

    void Start()
    {
        currentHealth = Mathf.Clamp(startHealth, 0, maxHealth); // startowe HP
        UpdateHearts();
    }

    void Update()
    {
        // tylko dla testów: zmiana HP przy klawiszach (mo¿esz usun¹æ)
        if (Input.GetKeyDown(KeyCode.Minus)) TakeDamage(1);
        if (Input.GetKeyDown(KeyCode.Equals)) Heal(1);
    }

    // Metoda zmieniaj¹ca serca w UI
    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
                hearts[i].SetActive(true);
            else
                hearts[i].SetActive(false);
        }
    }

    // Zadawanie obra¿eñ
    public void TakeDamage(int amount)
    {
        if (hitCooldown) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();

        // tutaj mo¿esz dodaæ knockback, efekty cz¹steczkowe itd.
        if (hitParticle != null) hitParticle.Play();

        hitCooldown = true;
        Invoke(nameof(ResetHitCooldown), 0.5f); // np. 0.5s invulnerability
    }

    void ResetHitCooldown()
    {
        hitCooldown = false;
    }

    // Leczenie
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();
    }

    // Zwiêkszenie maksymalnego HP (np. zbieranie power-upa)
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth = Mathf.Clamp(maxHealth + amount, 0, hearts.Length); // nie wiêcej ni¿ serc w UI
        Heal(amount); // opcjonalnie dodaj nowe HP
    }
}
