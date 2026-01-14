public interface IPersistable<T>
{
    void PopulateSaveData(T data);
    void LoadFromSaveData(T data);
}