public interface IStepAbility
{
    void OnRunnerStep(AbilityContext context, AbilitySnapshot snapshot, int index, AbilityRunner runner);
}