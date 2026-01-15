using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays warning indicators for ring attack projectiles.
/// Shows lines indicating where projectiles will travel.
/// </summary>
public class RingAttackIndicator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f, 0.6f);
    [SerializeField] private float lineWidth = 0.15f;
    [SerializeField] private float lineLength = 10f;
    [SerializeField] private float pulseSpeed = 5f;
    
    private List<LineRenderer> _lines = new List<LineRenderer>();
    private float _displayTimer;
    private float _displayDuration;
    private bool _isShowing;
    
    /// <summary>
    /// Show warning lines for ring attack.
    /// </summary>
    /// <param name="projectileCount">Number of projectiles</param>
    /// <param name="startAngle">Starting angle offset in degrees</param>
    /// <param name="duration">How long to show warning</param>
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
    
    /// <summary>
    /// Hide all warning lines.
    /// </summary>
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
        
        // Pulse effect
        float pulse = (Mathf.Sin(_displayTimer * pulseSpeed) + 1f) / 2f;
        float alpha = Mathf.Lerp(0.3f, 0.8f, pulse);
        
        // Lines get more intense as attack approaches
        alpha = Mathf.Lerp(alpha * 0.5f, alpha, progress);
        
        Color currentColor = warningColor;
        currentColor.a = alpha;
        
        foreach (var line in _lines)
        {
            if (line != null)
            {
                line.startColor = currentColor;
                line.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a * 0.3f);
                
                // Update position to follow boss
                line.SetPosition(0, transform.position);
                Vector2 dir = ((Vector2)line.GetPosition(1) - (Vector2)line.GetPosition(0)).normalized;
                line.SetPosition(1, (Vector2)transform.position + dir * lineLength);
            }
        }
        
        // Auto-hide
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
        line.sortingOrder = 99;
        
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
