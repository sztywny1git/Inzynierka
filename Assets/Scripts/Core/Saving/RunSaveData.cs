using System.Collections.Generic;

[System.Serializable]
public class RunSaveData
{
    public string CharacterClassID;
    public string RunDefinitionID;
    public int CurrentLevelIndex;
    public float CurrentHealth;
    public int RunGold;
    public List<string> ActiveAugmentIDs;
}