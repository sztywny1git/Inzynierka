using System;

public interface ICastAnimationHandler
{
    event Action<string, float> OnCastAnimationRequired;
    event Action OnCastInterrupted;

    void OnAnimAttackPoint();
    void OnAnimFinish();
}