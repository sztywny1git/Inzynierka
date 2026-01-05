using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Slider slider;

    public TMP_Text resourceText;

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
            slider.maxValue = playerStats.Resource.FinalValue;
            slider.value = playerStats.CurrentResource;
            resourceText.text = $"{playerStats.CurrentHealth:F0}/{playerStats.Health.FinalValue:F0}";

            // Pod³¹cz event
            playerStats.OnResourceChangedEvent += UpdateResourceSlider;
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnResourceChangedEvent += UpdateResourceSlider;
        }
    }

    private void UpdateResourceSlider(float currentResource, float maxResource)
    {
        slider.value = currentResource;
        slider.maxValue = maxResource;
        if (resourceText != null)
            resourceText.text = $"{currentResource:F0}/{maxResource:F0}";
    }

}
