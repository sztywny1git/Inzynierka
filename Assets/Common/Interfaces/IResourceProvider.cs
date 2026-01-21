using System;

public interface IResourceProvider
{
    float CurrentValue { get; }
    float MaxValue { get; }
    
    event Action<float, float> OnResourceChanged;

    bool HasEnough(float amount);
    void Consume(float amount);
    void Restore(float amount);
}