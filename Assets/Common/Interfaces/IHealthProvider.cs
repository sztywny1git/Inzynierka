using System;

public interface IHealthProvider
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsAlive { get; }

    event Action<bool> OnInvulnerabilityChanged; 
    event Action<float, float> OnHealthChanged;
    event Action<DamageData> OnDamageTaken;
    event Action Death;
}