using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    public static ResourceUI Instance;

    [Header("UI References")]
    public Slider resourceSlider;
    public TMP_Text resourceText;

    private IResourceProvider currentResource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterPlayerResource(IResourceProvider newResource)
    {
        if (currentResource != null)
        {
            currentResource.OnResourceChanged -= UpdateResourceUI;
        }

        currentResource = newResource;

        if (currentResource != null)
        {
            currentResource.OnResourceChanged += UpdateResourceUI;
            UpdateResourceUI(currentResource.CurrentValue, currentResource.MaxValue);
        }
    }

    private void UpdateResourceUI(float currentValue, float maxValue)
    {
        if (resourceSlider != null)
        {
            resourceSlider.maxValue = maxValue;
            resourceSlider.value = currentValue;
        }

        if (resourceText != null)
        {
            resourceText.text = $"{currentValue:F0} / {maxValue:F0}";
        }
    }
}