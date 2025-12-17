using UnityEngine;
using System.Collections.Generic;
using VContainer.Unity;

public class CooldownSystem : ICooldownProvider, ITickable
{
    private class CooldownData { public float Remaining; }
    private readonly Dictionary<(int, ActionIdentifier), CooldownData> _cooldowns = new Dictionary<(int, ActionIdentifier), CooldownData>();
    private readonly List<(int, ActionIdentifier)> _toRemove = new List<(int, ActionIdentifier)>();

    public void Tick()
    {
        _toRemove.Clear();
        foreach (var entry in _cooldowns)
        {
            entry.Value.Remaining -= Time.deltaTime;
            if (entry.Value.Remaining <= 0) _toRemove.Add(entry.Key);
        }
        foreach (var key in _toRemove) _cooldowns.Remove(key);
    }

    public bool IsOnCooldown(int ownerId, ActionIdentifier actionId)
    {
        return _cooldowns.ContainsKey((ownerId, actionId));
    }

    public void StartCooldown(int ownerId, ActionIdentifier actionId, float duration)
    {
        _cooldowns[(ownerId, actionId)] = new CooldownData { Remaining = duration };
    }
}