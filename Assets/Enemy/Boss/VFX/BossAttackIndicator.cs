using UnityEngine;

/// <summary>
/// Displays attack range indicators for boss attacks.
/// Shows a visual warning to players before attacks land.
/// </summary>
public class BossAttackIndicator : MonoBehaviour
{
    [Header("Indicator Prefab (Optional)")]
    [SerializeField] private GameObject indicatorPrefab;
    
    [Header("Settings")]
    [SerializeField] private Color fillColor = new Color(1f, 0f, 0f, 0.25f);
    [SerializeField] private Color borderColor = new Color(1f, 0f, 0f, 0.9f);
    [SerializeField] private float borderWidth = 0.15f;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float growSpeed = 8f;
    
    private GameObject _indicatorObject;
    private SpriteRenderer _fillRenderer;
    private SpriteRenderer _borderRenderer;
    private Transform _indicatorTransform;
    
    private float _displayTimer;
    private float _displayDuration;
    private float _targetRadius;
    private float _currentRadius;
    private bool _isShowing;
    
    private void Awake()
    {
        CreateIndicator();
    }
    
    private void Start()
    {
        // Ensure hidden at start
        HideIndicator();
    }
    
    private void CreateIndicator()
    {
        // Don't create if already exists
        if (_indicatorObject != null) return;
        
        // Main indicator object
        _indicatorObject = new GameObject("AttackIndicator");
        _indicatorObject.transform.SetParent(transform);
        _indicatorObject.transform.localPosition = Vector3.zero;
        _indicatorTransform = _indicatorObject.transform;
        
        // Create fill circle
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(_indicatorObject.transform);
        fillObj.transform.localPosition = Vector3.zero;
        _fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        _fillRenderer.sprite = CreateCircleSprite(64, true);
        _fillRenderer.sortingLayerName = "Default";
        _fillRenderer.sortingOrder = 100;
        _fillRenderer.color = fillColor;
        
        // Create border ring
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(_indicatorObject.transform);
        borderObj.transform.localPosition = Vector3.zero;
        _borderRenderer = borderObj.AddComponent<SpriteRenderer>();
        _borderRenderer.sprite = CreateRingSprite(64, 0.85f);
        _borderRenderer.sortingLayerName = "Default";
        _borderRenderer.sortingOrder = 101;
        _borderRenderer.color = borderColor;
    }
    
    private Sprite CreateCircleSprite(int resolution, bool filled)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.filterMode = FilterMode.Bilinear;
        
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f - 1f;
        
        Color[] pixels = new Color[resolution * resolution];
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                int index = y * resolution + x;
                
                if (distance <= radius)
                {
                    float alpha = filled ? 1f : 0f;
                    // Smooth edge
                    if (distance > radius - 2f)
                    {
                        alpha = filled ? (radius - distance) / 2f : 0f;
                    }
                    pixels[index] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[index] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // resolution pixels = 1 world unit, so scale 1 = 1 unit diameter
        return Sprite.Create(
            texture,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f),
            resolution
        );
    }
    
    private Sprite CreateRingSprite(int resolution, float innerRadiusPercent)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.filterMode = FilterMode.Bilinear;
        
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float outerRadius = resolution / 2f - 1f;
        float innerRadius = outerRadius * innerRadiusPercent;
        
        Color[] pixels = new Color[resolution * resolution];
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                int index = y * resolution + x;
                
                if (distance <= outerRadius && distance >= innerRadius)
                {
                    float alpha = 1f;
                    // Smooth outer edge
                    if (distance > outerRadius - 1.5f)
                    {
                        alpha = (outerRadius - distance) / 1.5f;
                    }
                    // Smooth inner edge
                    else if (distance < innerRadius + 1.5f)
                    {
                        alpha = (distance - innerRadius) / 1.5f;
                    }
                    pixels[index] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
                }
                else
                {
                    pixels[index] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // resolution pixels = 1 world unit, so scale 1 = 1 unit diameter
        return Sprite.Create(
            texture,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f),
            resolution
        );
    }
    
    private void Update()
    {
        if (!_isShowing) return;
        
        _displayTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(_displayTimer / _displayDuration);
        
        // Grow animation - starts small, grows to full size
        _currentRadius = Mathf.Lerp(_currentRadius, _targetRadius, Time.deltaTime * growSpeed);
        float scale = _currentRadius * 2f;
        _indicatorTransform.localScale = new Vector3(scale, scale, 1f);
        
        // Pulse effect - border pulses faster as attack approaches
        float pulseRate = pulseSpeed * (1f + progress * 3f);
        float pulse = (Mathf.Sin(_displayTimer * pulseRate) + 1f) / 2f;
        
        // Fill becomes more opaque as attack approaches
        Color currentFill = fillColor;
        currentFill.a = Mathf.Lerp(fillColor.a * 0.5f, fillColor.a * 1.5f, progress);
        _fillRenderer.color = currentFill;
        
        // Border pulses
        Color currentBorder = borderColor;
        currentBorder.a = Mathf.Lerp(0.5f, 1f, pulse);
        _borderRenderer.color = currentBorder;
        
        // Scale border slightly for pulse effect
        float borderPulse = 1f + pulse * 0.05f;
        _borderRenderer.transform.localScale = new Vector3(borderPulse, borderPulse, 1f);
    }
    
    /// <summary>
    /// Show circular attack indicator.
    /// </summary>
    public void ShowCircleIndicator(float radius, float duration)
    {
        ShowCircleIndicator(Vector3.zero, radius, duration);
    }
    
    /// <summary>
    /// Show circular attack indicator at offset position.
    /// </summary>
    public void ShowCircleIndicator(Vector3 offset, float radius, float duration)
    {
        _isShowing = true;
        _displayTimer = 0f;
        _displayDuration = duration;
        _targetRadius = radius;
        _currentRadius = radius * 0.3f; // Start small
        
        _indicatorTransform.localPosition = new Vector3(offset.x, offset.y, 0f);
        _indicatorObject.SetActive(true);
        
        // Enable renderers
        if (_fillRenderer != null)
        {
            _fillRenderer.enabled = true;
        }
        if (_borderRenderer != null)
        {
            _borderRenderer.enabled = true;
        }
        
        _fillRenderer.color = fillColor;
        _borderRenderer.color = borderColor;
    }
    
    /// <summary>
    /// Hide the indicator immediately.
    /// </summary>
    public void HideIndicator()
    {
        _isShowing = false;
        Debug.Log("[BossAttackIndicator] HideIndicator called");
        
        if (_indicatorObject != null)
        {
            _indicatorObject.SetActive(false);
        }
        
        // Also disable renderers directly as backup
        if (_fillRenderer != null)
        {
            _fillRenderer.enabled = false;
        }
        if (_borderRenderer != null)
        {
            _borderRenderer.enabled = false;
        }
    }
    
    private void OnDestroy()
    {
        if (_indicatorObject != null)
        {
            Destroy(_indicatorObject);
        }
    }
}
