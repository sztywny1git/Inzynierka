using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance;

    [Header("UI References")]
    public Slider healthSlider;
    public TMP_Text healthText;

    private IHealthProvider currentHealth;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterPlayerHealth(IHealthProvider newHealth)
    {
        if (currentHealth != null)
        {
            currentHealth.OnHealthChanged -= UpdateHealthUI;
        }

        currentHealth = newHealth;

        if (currentHealth != null)
        {
            currentHealth.OnHealthChanged += UpdateHealthUI;
            UpdateHealthUI(currentHealth.CurrentHealth, currentHealth.MaxHealth);
        }
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
        }
    }

    private void OnDestroy()
    {
        if (currentHealth != null)
        {
            currentHealth.OnHealthChanged -= UpdateHealthUI;
        }
    }
}