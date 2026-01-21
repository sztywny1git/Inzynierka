using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using System;
using System.Collections.Generic;

public class InteractionTooltip : MonoBehaviour, IDisposable
{
    [Serializable]
    public struct ButtonIcon
    {
        public string ActionName;
        public Sprite Icon;
    }
    
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private Image buttonIconImage;
    [SerializeField] private GameObject container;
    [SerializeField] private List<ButtonIcon> buttonIcons;
    
    [SerializeField] private string defaultActionName = "Interact"; 
    
    private Dictionary<string, Sprite> _iconMap;
    private UIEventBus _uiEventBus;

    [Inject]
    public void Construct(UIEventBus uiEventBus)
    {
        _uiEventBus = uiEventBus;
        _uiEventBus.InteractionTooltipUpdated += HandleUpdateTooltip;
    }
    
    private void Awake()
    {
        _iconMap = new Dictionary<string, Sprite>();
        foreach (var icon in buttonIcons)
        {
            _iconMap[icon.ActionName] = icon.Icon;
        }
    }

    public void Dispose()
    {
        if (_uiEventBus != null)
        {
            _uiEventBus.InteractionTooltipUpdated -= HandleUpdateTooltip;
        }
    }

    private void Start()
    {
        container.SetActive(false);
    }

    private void HandleUpdateTooltip(string text)
    {
        bool hasText = !string.IsNullOrEmpty(text);
        container.SetActive(hasText);

        if (hasText)
        {
            actionText.text = text;

            if (_iconMap.TryGetValue(defaultActionName, out Sprite icon))
            {
                buttonIconImage.sprite = icon;
                buttonIconImage.enabled = true;
            }
            else
            {
                buttonIconImage.enabled = false;
            }
        }
    }
}