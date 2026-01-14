using System.Threading.Tasks;
using System.Collections.Generic;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

public class MainMenuState : IGameState
{
    private readonly ISceneContextManager _sceneManager;
    private readonly UIEventBus _uiEventBus;
    private readonly SaveGameManager _saveManager;
    private readonly LifetimeScope _gameScope;

    public MainMenuState(
        ISceneContextManager sceneManager, 
        UIEventBus uiEventBus,
        SaveGameManager saveManager, 
        LifetimeScope gameScope)
    {
        _sceneManager = sceneManager;
        _uiEventBus = uiEventBus;
        _saveManager = saveManager;
        _gameScope = gameScope;
    }
    
    public async UniTask OnEnter()
    {

        using (LifetimeScope.EnqueueParent(_gameScope))
        {
            await _sceneManager.LoadSceneAsync("MainMenu");
        }

        var summaries = _saveManager.GetAllProfileSummaries();
        
        _uiEventBus.PublishProfileList(summaries);
    }

    public UniTask OnExit()
    {
        return UniTask.CompletedTask;
    }
}