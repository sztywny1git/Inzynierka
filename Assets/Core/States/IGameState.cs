using Cysharp.Threading.Tasks;

public interface IGameState
{
    UniTask OnEnter();
    UniTask OnExit();
}