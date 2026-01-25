using System.Collections.Generic;
using UnityEngine;

public class RingAttackIndicator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f, 0.6f);
    [SerializeField] private float lineWidth = 0.15f;
    [SerializeField] private float lineLength = 10f;
    [SerializeField] private float pulseSpeed = 5f;
    
    private string _sortingLayerName = "Default";
    private int _sortingOrder = 100;
    
    private List<LineRenderer> _lines = new List<LineRenderer>();
    private float _displayTimer;
    private float _displayDuration;
    private bool _isShowing;

    public void ConfigureSorting(string layerName, int order)
    {
        _sortingLayerName = layerName;
        _sortingOrder = order;
        
        foreach(var line in _lines)
        {
            if (line != null)
            {
                line.sortingLayerName = layerName;
                line.sortingOrder = order;
            }
        }
    }
    
    public void ShowRingWarning(int projectileCount, float startAngle, float duration)
    {
        ClearLines();
        
        _isShowing = true;
        _displayTimer = 0f;
        _displayDuration = duration;
        
        float angleStep = 360f / projectileCount;
        
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = AngleToDirection(angle);
            
            LineRenderer line = CreateLine();
            line.SetPosition(0, transform.position);
            line.SetPosition(1, (Vector2)transform.position + direction * lineLength);
            _lines.Add(line);
        }
    }
    
    public void HideWarning()
    {
        _isShowing = false;
        ClearLines();
    }
    
    private void Update()
    {
        if (!_isShowing) return;
        
        _displayTimer += Time.deltaTime;
        float progress = _displayTimer / _displayDuration;
        
        float pulse = (Mathf.Sin(_displayTimer * pulseSpeed) + 1f) / 2f;
        float alpha = Mathf.Lerp(0.3f, 0.8f, pulse);
        alpha = Mathf.Lerp(alpha * 0.5f, alpha, progress);
        
        Color currentColor = warningColor;
        currentColor.a = alpha;
        
        foreach (var line in _lines)
        {
            if (line != null)
            {
                line.startColor = currentColor;
                line.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a * 0.3f);
                
                line.SetPosition(0, transform.position);
                Vector2 dir = ((Vector2)line.GetPosition(1) - (Vector2)line.GetPosition(0)).normalized;
                line.SetPosition(1, (Vector2)transform.position + dir * lineLength);
            }
        }
        
        if (_displayTimer >= _displayDuration)
        {
            HideWarning();
        }
    }
    
    private LineRenderer CreateLine()
    {
        GameObject lineObj = new GameObject("WarningLine");
        lineObj.transform.SetParent(transform);
        
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth * 0.5f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = warningColor;
        line.endColor = new Color(warningColor.r, warningColor.g, warningColor.b, 0.2f);
        
        line.sortingLayerName = _sortingLayerName;
        line.sortingOrder = _sortingOrder;
        
        return line;
    }
    
    private void ClearLines()
    {
        foreach (var line in _lines)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        _lines.Clear();
    }
    
    private Vector2 AngleToDirection(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
    
    private void OnDestroy()
    {
        ClearLines();
    }
}