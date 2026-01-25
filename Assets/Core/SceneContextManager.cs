using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using System;

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
        var oldScene = _currentScene;

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

        if (oldScene.IsValid())
        {
            await SceneManager.UnloadSceneAsync(oldScene);
        }

        await Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private async UniTask LoadSceneInternal(string sceneName)
    {
        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        _currentScene = SceneManager.GetSceneByName(sceneName);
        
        SceneManager.SetActiveScene(_currentScene);
    }
}