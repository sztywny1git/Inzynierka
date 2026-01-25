using UnityEngine;

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
    
    private string _sortingLayerName = "Default";
    private int _sortingOrder = 100;
    
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
        HideIndicator();
    }

    public void ConfigureSorting(string layerName, int order)
    {
        _sortingLayerName = layerName;
        _sortingOrder = order;

        if (_fillRenderer != null)
        {
            _fillRenderer.sortingLayerName = layerName;
            _fillRenderer.sortingOrder = order;
        }

        if (_borderRenderer != null)
        {
            _borderRenderer.sortingLayerName = layerName;
            _borderRenderer.sortingOrder = order + 1;
        }
    }
    
    private void CreateIndicator()
    {
        if (_indicatorObject != null) return;
        
        _indicatorObject = new GameObject("AttackIndicator");
        _indicatorObject.transform.SetParent(transform);
        _indicatorObject.transform.localPosition = Vector3.zero;
        _indicatorTransform = _indicatorObject.transform;
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(_indicatorObject.transform);
        fillObj.transform.localPosition = Vector3.zero;
        _fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        _fillRenderer.sprite = CreateCircleSprite(64, true);
        _fillRenderer.sortingLayerName = _sortingLayerName;
        _fillRenderer.sortingOrder = _sortingOrder;
        _fillRenderer.color = fillColor;
        
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(_indicatorObject.transform);
        borderObj.transform.localPosition = Vector3.zero;
        _borderRenderer = borderObj.AddComponent<SpriteRenderer>();
        _borderRenderer.sprite = CreateRingSprite(64, 0.85f);
        _borderRenderer.sortingLayerName = _sortingLayerName;
        _borderRenderer.sortingOrder = _sortingOrder + 1;
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
                    if (distance > outerRadius - 1.5f)
                    {
                        alpha = (outerRadius - distance) / 1.5f;
                    }
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
        
        _currentRadius = Mathf.Lerp(_currentRadius, _targetRadius, Time.deltaTime * growSpeed);
        float scale = _currentRadius * 2f;
        _indicatorTransform.localScale = new Vector3(scale, scale, 1f);
        
        float pulseRate = pulseSpeed * (1f + progress * 3f);
        float pulse = (Mathf.Sin(_displayTimer * pulseRate) + 1f) / 2f;
        
        Color currentFill = fillColor;
        currentFill.a = Mathf.Lerp(fillColor.a * 0.5f, fillColor.a * 1.5f, progress);
        _fillRenderer.color = currentFill;
        
        Color currentBorder = borderColor;
        currentBorder.a = Mathf.Lerp(0.5f, 1f, pulse);
        _borderRenderer.color = currentBorder;
        
        float borderPulse = 1f + pulse * 0.05f;
        _borderRenderer.transform.localScale = new Vector3(borderPulse, borderPulse, 1f);
    }
    
    public void ShowCircleIndicator(float radius, float duration)
    {
        ShowCircleIndicator(Vector3.zero, radius, duration);
    }
    
    public void ShowCircleIndicator(Vector3 offset, float radius, float duration)
    {
        _isShowing = true;
        _displayTimer = 0f;
        _displayDuration = duration;
        _targetRadius = radius;
        _currentRadius = radius * 0.3f;
        
        _indicatorTransform.localPosition = new Vector3(offset.x, offset.y, 0f);
        _indicatorObject.SetActive(true);
        
        if (_fillRenderer != null) _fillRenderer.enabled = true;
        if (_borderRenderer != null) _borderRenderer.enabled = true;
        
        _fillRenderer.color = fillColor;
        _borderRenderer.color = borderColor;
    }
    
    public void HideIndicator()
    {
        _isShowing = false;
        
        if (_indicatorObject != null)
        {
            _indicatorObject.SetActive(false);
        }
        
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