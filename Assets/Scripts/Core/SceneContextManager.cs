using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneContextManager : ISceneContextManager
{
    private Scene _currentScene;

    public async Task LoadSceneAsync(string sceneName)
    {
        if (_currentScene.IsValid())
        {
            var unloadOperation = SceneManager.UnloadSceneAsync(_currentScene);
            while (!unloadOperation.isDone)
            {
                await Task.Yield();
            }
        }

        var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOperation.isDone)
        {
            await Task.Yield();
        }
        _currentScene = SceneManager.GetSceneByName(sceneName);
    }
}