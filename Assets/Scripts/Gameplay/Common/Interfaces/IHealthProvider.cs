using System;
using UnityEngine;

public interface IHealthProvider
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    event Action<float, float> OnHealthChanged;
}
