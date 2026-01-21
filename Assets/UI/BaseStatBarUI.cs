using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

public abstract class BaseStatBarUI : MonoBehaviour
{
    [SerializeField] protected Slider barSlider;
    [SerializeField] protected TextMeshProUGUI valueText;

    protected UIEventBus UiEventBus;

    [Inject]
    public void Construct(UIEventBus uiEventBus)
    {
        UiEventBus = uiEventBus;
    }

    protected virtual void UpdateDisplay(float current, float max)
    {
        if (barSlider != null)
        {
            barSlider.value = max > 0 ? current / max : 0;
        }

        if (valueText != null)
        {
            valueText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }
}