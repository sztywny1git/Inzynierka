using UnityEngine;

public class CasterVisuals : MonoBehaviour
{
    [SerializeField] private AbilityCaster _caster;
    [SerializeField] private Animator _animator;

    private void OnEnable()
    {
        _caster.OnCastAnimationRequired += PlayAnimation;
    }

    private void OnDisable()
    {
        _caster.OnCastAnimationRequired -= PlayAnimation;
    }

    private void PlayAnimation(string triggerName)
    {
        if (!string.IsNullOrEmpty(triggerName))
        {
            _animator.SetTrigger(triggerName);
        }
    }

    public void OnAnimAttackPoint()
    {
        _caster.ReleaseSpell();
    }
}