using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

public class SceneContextManager : ISceneContextManager
{
    private readonly GameScopeService _gameScopeService;
    private Scene _currentScene;

    public SceneContextManager(GameScopeService gameScopeService)
    {
        _gameScopeService = gameScopeService;
    }

    public async UniTask LoadSceneAsync(string sceneName)
    {
        if (_currentScene.IsValid())
        {
            await SceneManager.UnloadSceneAsync(_currentScene);
        }

        var parentScope = _gameScopeService.GetActiveScope();

        if (parentScope != null)
        {
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                await LoadSceneInternal(sceneName);
            }
        }
        else
        {
            await LoadSceneInternal(sceneName);
        }
    }

    private async UniTask LoadSceneInternal(string sceneName)
    {
        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _currentScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(_currentScene);
    }
}