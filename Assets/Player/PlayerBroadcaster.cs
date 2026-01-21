using UnityEngine;
using VContainer;

[RequireComponent(typeof(IHealthProvider))]
public class PlayerBroadcaster : MonoBehaviour
{
    private IHealthProvider _healthProvider;
    private IResourceProvider _resourceProvider;
    private UIEventBus _uiEventBus;

    [Inject]
    public void Construct(UIEventBus uiBus)
    {
        _uiEventBus = uiBus;
    }

    private void Start()
    {
        _healthProvider = GetComponent<IHealthProvider>();
        _resourceProvider = GetComponent<IResourceProvider>();

        if (_uiEventBus != null)
        {
            _uiEventBus.RequestHUDVisibility(true);
        }

        if (_healthProvider != null)
        {
            _healthProvider.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(_healthProvider.CurrentHealth, _healthProvider.MaxHealth);
        }

        if (_resourceProvider != null)
        {
            _resourceProvider.OnResourceChanged += HandleResourceChanged;
            HandleResourceChanged(_resourceProvider.CurrentValue, _resourceProvider.MaxValue);
        }
    }

    private void OnDestroy()
    {
        if (_healthProvider != null)
        {
            _healthProvider.OnHealthChanged -= HandleHealthChanged;
        }

        if (_resourceProvider != null)
        {
            _resourceProvider.OnResourceChanged -= HandleResourceChanged;
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        _uiEventBus?.PublishPlayerHealthUpdate(current, max);
    }

    private void HandleResourceChanged(float current, float max)
    {
        _uiEventBus?.PublishPlayerResourceUpdate(current, max);
    }
}