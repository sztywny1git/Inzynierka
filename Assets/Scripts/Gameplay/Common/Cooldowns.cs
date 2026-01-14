using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IStatsProvider))]
public class Cooldowns : MonoBehaviour, ICooldownProvider
{
    [Header("Stats Definitions")]
    [SerializeField] private StatDefinition _cooldownReductionStatDef;

    private IStatsProvider _statsProvider;
    private readonly Dictionary<ActionIdentifier, float> _cooldownEndTimes = new Dictionary<ActionIdentifier, float>();

    private void Awake()
    {
        _statsProvider = GetComponent<IStatsProvider>();
    }

    public bool IsOnCooldown(ActionIdentifier actionId)
    {
        if (actionId == null) return false;
        if (_cooldownEndTimes.TryGetValue(actionId, out float endTime))
        {
            return Time.time < endTime;
        }
        return false;
    }

    public float GetRemainingDuration(ActionIdentifier actionId)
    {
        if (actionId != null && _cooldownEndTimes.TryGetValue(actionId, out float endTime))
        {
            return Mathf.Max(0f, endTime - Time.time);
        }
        return 0f;
    }

    public void PutOnCooldown(ActionIdentifier actionId, float baseDuration)
    {
        if (actionId == null) return;

        float reductionPercent = 0f;
        
        if (_statsProvider != null && _cooldownReductionStatDef != null)
        {
            reductionPercent = _statsProvider.GetFinalStatValue(_cooldownReductionStatDef);
            reductionPercent = Mathf.Clamp(reductionPercent, 0f, 100f);
        }

        float multiplier = 1.0f - (reductionPercent / 100f);
        float finalDuration = baseDuration * multiplier;
        
        _cooldownEndTimes[actionId] = Time.time + finalDuration;
    }

    public void ResetCooldown(ActionIdentifier actionId)
    {
        if (actionId != null && _cooldownEndTimes.ContainsKey(actionId))
        {
            _cooldownEndTimes.Remove(actionId);
        }
    }
}