using Cysharp.Threading.Tasks;

public enum GameStateId
{
    Boot,
    MainMenu,
    Hub,
    Run,
    RunSummary
}

public interface IGameState
{
    UniTask OnEnter();
    UniTask OnExit();
}