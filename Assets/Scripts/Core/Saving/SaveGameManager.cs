using System.Collections.Generic;

public class SaveGameManager
{
    private readonly SaveDataRepository _repository;
    private readonly GameConstants _gameConstants;
    private int _activeProfileSlot = -1;

    public SaveGameManager(SaveDataRepository repository, GameConstants gameConstants)
    {
        _repository = repository;
        _gameConstants = gameConstants;
    }

    public void SetActiveProfile(int slotId) => _activeProfileSlot = slotId;
    public int GetActiveProfileId() => _activeProfileSlot;

    public List<ProfileSummary> GetAllProfileSummaries()
    {
        var summaries = new List<ProfileSummary>();
        for (int i = 0; i < _gameConstants.MaxProfileSlots; i++)
        {
            _repository.TryLoad(GetMetaSavePath(i), out MetaSaveData metaData);
            _repository.TryLoad(GetRunSavePath(i), out RunSaveData runData);
            summaries.Add(new ProfileSummary(i, metaData, runData));
        }
        return summaries;
    }
    
    public void DeleteProfile(int slotId)
    {
        _repository.Delete(GetMetaSavePath(slotId));
        _repository.Delete(GetRunSavePath(slotId));
    }
    
    public MetaSaveData LoadMetaProgress()
    {
        _repository.TryLoad(GetMetaSavePath(_activeProfileSlot), out MetaSaveData data);
        return data;
    }

    public RunSaveData LoadRunState()
    {
        _repository.TryLoad(GetRunSavePath(_activeProfileSlot), out RunSaveData data);
        return data;
    }

    public void SaveMetaProgress(MetaSaveData data)
    {
        _repository.Save(GetMetaSavePath(_activeProfileSlot), data);
    }
    
    public void SaveRunState(RunSaveData data)
    {
        _repository.Save(GetRunSavePath(_activeProfileSlot), data);
    }
    
    public bool HasMetaSave(int slotId)
    {
        return _repository.Exists(GetMetaSavePath(slotId));
    }
    
    public bool HasRunSave(int slotId)
    {
        return _repository.Exists(GetRunSavePath(slotId));
    }

    private string GetMetaSavePath(int slotId) => $"Profile_{slotId}/meta_save.json";
    private string GetRunSavePath(int slotId) => $"Profile_{slotId}/run_save.json";
}