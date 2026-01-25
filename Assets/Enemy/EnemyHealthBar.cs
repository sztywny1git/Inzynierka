using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _foregroundImage;
    [SerializeField] private Image _ghostImage;
    [SerializeField] private TextMeshProUGUI _healthText;

    [Header("Settings")]
    [SerializeField] private float _ghostDelay = 0.5f;
    [SerializeField] private float _ghostSpeed = 2f;
    [SerializeField] private float _visibleDuration = 3f;
    [SerializeField] private float _fadeSpeed = 4f;
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0);

    private Health _healthComponent;
    private float _targetFill = 1f;
    private float _ghostTimer;
    private float _visibilityTimer;
    private Vector3 _originalScale;

    private void Awake()
    {
        _healthComponent = GetComponentInParent<Health>();
        
        _originalScale = transform.localScale; 
        
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        if (_healthComponent != null)
        {
            _healthComponent.OnHealthChanged += HandleHealthChanged;
            // Aktualizujemy pasek od razu po włączeniu, ale bez animacji śmierci
            if (_healthComponent.CurrentHealth > 0)
            {
                HandleHealthChanged(_healthComponent.CurrentHealth, _healthComponent.MaxHealth);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        if (_healthComponent != null)
        {
            _healthComponent.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void LateUpdate()
    {
        if (transform.parent != null)
        {
            transform.position = transform.parent.position + _offset;

            transform.rotation = Quaternion.identity;

            Vector3 parentScale = transform.parent.localScale;
            
            float newX = (parentScale.x < 0) ? -_originalScale.x : _originalScale.x;
            
            transform.localScale = new Vector3(newX, _originalScale.y, _originalScale.z);
        }

        UpdateGhost();
        UpdateVisibility();
    }

    private void HandleHealthChanged(float current, float max)
    {
        // 1. Jeśli zdrowie spadło do zera, natychmiast wyłączamy pasek
        if (current <= 0)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            return;
        }

        _targetFill = current / max;
        
        if (_foregroundImage != null)
        {
            _foregroundImage.fillAmount = _targetFill;
        }

        if (_healthText != null)
        {
            _healthText.text = $"{current:0}/{max:0}";
        }

        // 2. Jeśli żyje, pokazujemy pasek i resetujemy timer
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _visibilityTimer = _visibleDuration;
        }

        _ghostTimer = _ghostDelay;
    }

    private void UpdateGhost()
    {
        if (_ghostImage == null) return;

        if (_ghostTimer > 0)
        {
            _ghostTimer -= Time.deltaTime;
        }
        else if (_ghostImage.fillAmount > _targetFill)
        {
            _ghostImage.fillAmount = Mathf.Lerp(_ghostImage.fillAmount, _targetFill, Time.deltaTime * _ghostSpeed);
        }
        else if (_ghostImage.fillAmount < _targetFill)
        {
            _ghostImage.fillAmount = _targetFill;
        }
    }

    private void UpdateVisibility()
    {
        if (_canvasGroup == null) return;

        if (_visibilityTimer > 0)
        {
            _visibilityTimer -= Time.deltaTime;
        }
        else if (_canvasGroup.alpha > 0)
        {
            _canvasGroup.alpha -= Time.deltaTime * _fadeSpeed;
        }
    }
}