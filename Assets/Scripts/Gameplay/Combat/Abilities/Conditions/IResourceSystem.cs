public interface IResourceSystem
{
    string ResourceName { get; }
    
    bool CanAfford(float cost);
    void Consume(float amount);
}