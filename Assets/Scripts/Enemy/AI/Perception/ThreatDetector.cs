using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects incoming threats (projectiles, attacks) and provides evasion data.
/// Attach to enemy root. Uses Physics2D overlap to find nearby projectiles.
/// </summary>
public class ThreatDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 4f;        // How far to detect projectiles
    [SerializeField] private float detectionInterval = 0.15f;   // Check frequency
    [SerializeField] private LayerMask projectileLayer;         // Assign in inspector
    [SerializeField] private LayerMask playerAttackLayer;       // Assign in inspector
    
    [Header("Threat Assessment")]
    [SerializeField] private float dangerTimeThreshold = 0.6f;  // React when projectile this close (seconds)
    [SerializeField] private float minThreatSpeed = 2f;         // Ignore slow-moving objects
    
    [Header("Debug")]
    [SerializeField] private bool debugDraw = false;

    private float _nextDetectionTime;
    private readonly List<ThreatInfo> _activeThreats = new List<ThreatInfo>();
    private readonly Collider2D[] _overlapBuffer = new Collider2D[16];

    public IReadOnlyList<ThreatInfo> ActiveThreats => _activeThreats;
    public bool HasDangerousThreats => _activeThreats.Count > 0;
    
    /// <summary>
    /// Returns the most dangerous threat (closest time to impact).
    /// </summary>
    public ThreatInfo? MostDangerousThreat
    {
        get
        {
            if (_activeThreats.Count == 0) return null;
            
            ThreatInfo best = _activeThreats[0];
            for (int i = 1; i < _activeThreats.Count; i++)
            {
                if (_activeThreats[i].TimeToImpact < best.TimeToImpact)
                {
                    best = _activeThreats[i];
                }
            }
            return best;
        }
    }

    private void Update()
    {
        if (Time.time < _nextDetectionTime) return;
        _nextDetectionTime = Time.time + detectionInterval;
        
        DetectThreats();
    }

    private void DetectThreats()
    {
        _activeThreats.Clear();
        
        Vector2 myPos = transform.position;
        
        // Detect projectiles
        int count = Physics2D.OverlapCircleNonAlloc(myPos, detectionRadius, _overlapBuffer, projectileLayer | playerAttackLayer);
        
        for (int i = 0; i < count; i++)
        {
            var col = _overlapBuffer[i];
            if (col == null) continue;
            if (col.gameObject == gameObject) continue;
            
            // Get velocity from Rigidbody2D
            var rb = col.attachedRigidbody;
            if (rb == null) continue;
            
            Vector2 velocity = rb.linearVelocity;
            if (velocity.sqrMagnitude < minThreatSpeed * minThreatSpeed) continue;
            
            Vector2 threatPos = col.transform.position;
            Vector2 toMe = myPos - threatPos;
            
            // Check if projectile is heading towards us
            float dot = Vector2.Dot(velocity.normalized, toMe.normalized);
            if (dot < 0.3f) continue; // Not heading towards us
            
            // Calculate time to impact
            float distance = toMe.magnitude;
            float speed = velocity.magnitude;
            float timeToImpact = distance / speed;
            
            if (timeToImpact > dangerTimeThreshold) continue;
            
            // Calculate best dodge direction (perpendicular to threat velocity)
            Vector2 dodgeDir = Vector2.Perpendicular(velocity.normalized);
            
            // Choose the dodge direction that moves us away from threat's path
            Vector2 predictedImpact = threatPos + velocity * timeToImpact;
            if (Vector2.Dot(dodgeDir, myPos - predictedImpact) < 0)
            {
                dodgeDir = -dodgeDir;
            }
            
            _activeThreats.Add(new ThreatInfo
            {
                Source = col.transform,
                Position = threatPos,
                Velocity = velocity,
                TimeToImpact = timeToImpact,
                DodgeDirection = dodgeDir,
                ThreatLevel = CalculateThreatLevel(timeToImpact, speed)
            });
        }
        
        // Sort by danger (lowest time to impact first)
        _activeThreats.Sort((a, b) => a.TimeToImpact.CompareTo(b.TimeToImpact));
    }

    private float CalculateThreatLevel(float timeToImpact, float speed)
    {
        // Higher threat = lower time + higher speed
        float timeFactor = Mathf.Clamp01(1f - timeToImpact / dangerTimeThreshold);
        float speedFactor = Mathf.Clamp01(speed / 20f);
        return timeFactor * 0.7f + speedFactor * 0.3f;
    }

    /// <summary>
    /// Gets the best dodge direction considering all threats.
    /// </summary>
    public Vector2 GetBestDodgeDirection()
    {
        if (_activeThreats.Count == 0) return Vector2.zero;
        
        // Weight dodge directions by threat level
        Vector2 combinedDodge = Vector2.zero;
        float totalWeight = 0f;
        
        foreach (var threat in _activeThreats)
        {
            float weight = threat.ThreatLevel;
            combinedDodge += threat.DodgeDirection * weight;
            totalWeight += weight;
        }
        
        if (totalWeight > 0.001f)
        {
            combinedDodge /= totalWeight;
        }
        
        return combinedDodge.normalized;
    }

    /// <summary>
    /// Check if we should dodge right now based on imminent threats.
    /// </summary>
    public bool ShouldDodgeNow(float reactionTime = 0.5f)
    {
        foreach (var threat in _activeThreats)
        {
            if (threat.TimeToImpact <= reactionTime && threat.ThreatLevel > 0.5f)
            {
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!debugDraw) return;
        
        // Detection radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Active threats
        Gizmos.color = Color.red;
        foreach (var threat in _activeThreats)
        {
            Gizmos.DrawLine(transform.position, threat.Position);
            Gizmos.DrawWireSphere(threat.Position, 0.3f);
            
            // Dodge direction
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, threat.DodgeDirection * 2f);
            Gizmos.color = Color.red;
        }
    }
#endif
}

public struct ThreatInfo
{
    public Transform Source;
    public Vector2 Position;
    public Vector2 Velocity;
    public float TimeToImpact;
    public Vector2 DodgeDirection;
    public float ThreatLevel; // 0-1
}
