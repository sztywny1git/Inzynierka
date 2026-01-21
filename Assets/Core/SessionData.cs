public class SessionData
{
    public CharacterDefinition CurrentPlayerClass { get; set; }
    public int CurrentLevelIndex { get; set; } = -1;
    public RunOutcome CurrentRunOutcome { get; set; } = RunOutcome.Unknown;

    public void ResetRunData()
    {
        CurrentLevelIndex = -1;
        CurrentRunOutcome = RunOutcome.Unknown;
    }
}