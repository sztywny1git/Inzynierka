using Cysharp.Threading.Tasks;

public interface ISceneContextManager
{
    UniTask LoadSceneAsync(string sceneName);
}