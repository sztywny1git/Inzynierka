public class SessionData
{
    public CharacterDefinition CurrentPlayerClass { get; set; }
    public RunDefinition CurrentRun { get; set; }
    public int CurrentLevelIndex { get; set; } = -1;
    public LevelDefinition CurrentLevelDefinition { get; set; }
    public RunOutcome CurrentRunOutcome { get; set; } = RunOutcome.Unknown;
    public void ResetRunData()
    {
        CurrentRun = null;
        CurrentLevelIndex = -1;
        CurrentLevelDefinition = null;
        CurrentRunOutcome = RunOutcome.Unknown;
    }
}