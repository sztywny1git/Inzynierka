using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
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

    [Header("Health")]
    public int currentHealth;
    public int maxHealth;

    public HealthDisplay healthDisplay;



    void Start()
    {
        currentHealth = StatsManager.Instance.currentHearts;
        maxHealth = StatsManager.Instance.maxHearts;

        UpdateHearts();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Minus)) TakeDamage(1);
        if (Input.GetKeyDown(KeyCode.Equals)) Heal(1);
    }

    private void SynchronizacjaStatsManager()
    {
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.currentHearts = currentHealth;
            StatsManager.Instance.maxHearts = maxHealth;
        }
    }


    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SynchronizacjaStatsManager();

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }

        UpdateHearts();
    }


    public void TakeDamage(int amount)
    {
        if (hitCooldown) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SynchronizacjaStatsManager();

        if (hitParticle != null) hitParticle.Play();

        UpdateHearts();

        if (currentHealth <= 0)
            gameObject.SetActive(false);


        hitCooldown = true;
        Invoke(nameof(ResetHitCooldown), 0.5f);
    }

    void ResetHitCooldown()
    {
        hitCooldown = false;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SynchronizacjaStatsManager();

        UpdateHearts();
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth = Mathf.Clamp(maxHealth + amount, 0, healthDisplay.hearts.Length);
        Heal(amount);
    }

    void UpdateHearts()
    {
        if (healthDisplay != null)
        {
            healthDisplay.UpdateHearts(currentHealth, maxHealth);
        }
    }
}
