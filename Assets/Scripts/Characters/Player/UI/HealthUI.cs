using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Slider slider;

    public TMP_Text healthText;

    private void Start()
    {
        // ZnajdŸ PlayerStats jeœli nie jest przypisany
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats != null)
        {
            // Ustaw pocz¹tkow¹ wartoœæ slidera
            slider.maxValue = playerStats.Health.FinalValue;
            slider.value = playerStats.CurrentHealth;
            healthText.text = $"{playerStats.CurrentHealth:F0}/{playerStats.Health.FinalValue:F0}";

            // Pod³¹cz event
            playerStats.OnHealthChangedEvent += UpdateHealthSlider;
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChangedEvent += UpdateHealthSlider;
        }
    }

    private void UpdateHealthSlider(float currentHealth, float maxHealth)
    {
        slider.value = currentHealth;
        slider.maxValue = maxHealth;
        if (healthText != null)
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
    }

}
