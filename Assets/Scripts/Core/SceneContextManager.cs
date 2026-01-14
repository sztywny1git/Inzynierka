using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class SceneContextManager : ISceneContextManager
{
    private Scene _currentScene;

    public async UniTask LoadSceneAsync(string sceneName)
    {
        if (_currentScene.IsValid())
        {
            var unloadOperation = SceneManager.UnloadSceneAsync(_currentScene);
            while (!unloadOperation.isDone) await UniTask.Yield();
        }

        var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOperation.isDone) await UniTask.Yield();
        
        _currentScene = SceneManager.GetSceneByName(sceneName);
    }
}