using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "ChangeClassInteraction", menuName = "Interaction/Change Class")]
public class ChangeClassInteraction : Interaction
{
    [SerializeField] private CharacterDefinition classToAssign;

    public override void Execute(InteractionContext context)
    {
        // 1. Pobieramy potrzebne serwisy
        var transitionService = context.DIContainer.Resolve<ScreenTransitionService>();
        var gameplayBus = context.DIContainer.Resolve<GameplayEventBus>();

        // 2. Uruchamiamy proces "fire-and-forget", bo Execute jest void
        PerformClassChange(transitionService, gameplayBus).Forget();
    }

    private async UniTaskVoid PerformClassChange(ScreenTransitionService transitionService, GameplayEventBus gameplayBus)
    {
        // 3. Używamy serwisu do obsługi Fadera
        await transitionService.PerformTransition(async () => 
        {
            // Ta część wykona się, gdy ekran będzie całkowicie czarny
            gameplayBus.InvokeClassSelected(classToAssign);
            
            // Opcjonalnie: możemy poczekać klatkę, żeby upewnić się, 
            // że model postaci zdążył się podmienić zanim ekran się rozjaśni
            await UniTask.Yield();
        });
    }
}