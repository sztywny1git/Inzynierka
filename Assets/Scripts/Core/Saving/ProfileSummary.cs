public class ProfileSummary
{
    public readonly int SlotId;
    public readonly bool IsOccupied;
    public readonly bool HasActiveRun;
    
    public readonly string CharacterClassName;
    public readonly int CurrentLevel;

    public ProfileSummary(int slotId, MetaSaveData metaData, RunSaveData runData)
    {
        SlotId = slotId;
        IsOccupied = metaData != null;
        HasActiveRun = runData != null;

        if (IsOccupied)
        {
            CharacterClassName = metaData.LastUsedCharacterClassID;
        }
        if (HasActiveRun)
        {
            CurrentLevel = runData.CurrentLevelIndex;
        }
    }
}