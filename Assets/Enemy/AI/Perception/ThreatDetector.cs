using System.Collections.Generic;
using UnityEngine;

public class ThreatDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float detectionInterval = 0.15f;
    [SerializeField] private LayerMask projectileLayer;
    [SerializeField] private LayerMask playerAttackLayer;
    
    [Header("Threat Assessment")]
    [SerializeField] private float dangerTimeThreshold = 0.6f;
    [SerializeField] private float minThreatSpeed = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool debugDraw = false;

    private float _nextDetectionTime;
    private readonly List<ThreatInfo> _activeThreats = new List<ThreatInfo>();
    private readonly Collider2D[] _overlapBuffer = new Collider2D[16];

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
        
        int count = Physics2D.OverlapCircleNonAlloc(myPos, detectionRadius, _overlapBuffer, projectileLayer | playerAttackLayer);
        
        for (int i = 0; i < count; i++)
        {
            var col = _overlapBuffer[i];
            if (col == null) continue;
            if (col.gameObject == gameObject) continue;
            
            var rb = col.attachedRigidbody;
            if (rb == null) continue;
            
            Vector2 velocity = rb.linearVelocity;
            if (velocity.sqrMagnitude < minThreatSpeed * minThreatSpeed) continue;
            
            Vector2 threatPos = col.transform.position;
            Vector2 toMe = myPos - threatPos;
            
            float dot = Vector2.Dot(velocity.normalized, toMe.normalized);
            if (dot < 0.3f) continue;
            
            float distance = toMe.magnitude;
            float speed = velocity.magnitude;
            float timeToImpact = distance / speed;
            
            if (timeToImpact > dangerTimeThreshold) continue;
            
            Vector2 dodgeDir = Vector2.Perpendicular(velocity.normalized);
            
            Vector2 predictedImpact = threatPos + velocity * timeToImpact;
            if (Vector2.Dot(dodgeDir, myPos - predictedImpact) < 0)
            {
                dodgeDir = -dodgeDir;
            }
            
            _activeThreats.Add(new ThreatInfo
            {
                Position = threatPos,
                Velocity = velocity,
                TimeToImpact = timeToImpact,
                DodgeDirection = dodgeDir,
                ThreatLevel = CalculateThreatLevel(timeToImpact, speed)
            });
        }
        
        _activeThreats.Sort((a, b) => a.TimeToImpact.CompareTo(b.TimeToImpact));
    }

    private float CalculateThreatLevel(float timeToImpact, float speed)
    {
        float timeFactor = Mathf.Clamp01(1f - timeToImpact / dangerTimeThreshold);
        float speedFactor = Mathf.Clamp01(speed / 20f);
        return timeFactor * 0.7f + speedFactor * 0.3f;
    }

    public Vector2 GetBestDodgeDirection()
    {
        if (_activeThreats.Count == 0) return Vector2.zero;
        
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
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        foreach (var threat in _activeThreats)
        {
            Gizmos.DrawLine(transform.position, threat.Position);
            Gizmos.DrawWireSphere(threat.Position, 0.3f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, threat.DodgeDirection * 2f);
            Gizmos.color = Color.red;
        }
    }
#endif
}

public struct ThreatInfo
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float TimeToImpact;
    public Vector2 DodgeDirection;
    public float ThreatLevel;
}
