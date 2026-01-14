using UnityEngine;

public interface IHealthProvider
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
}
