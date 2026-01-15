using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CasterVisuals : MonoBehaviour
{
    [SerializeField] private AbilityCaster _caster;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_caster == null) _caster = GetComponentInParent<AbilityCaster>();
    }

    private void OnEnable()
    {
        if (_caster != null)
        {
            _caster.OnCastAnimationRequired += PlayAnimation;
            _caster.OnCastInterrupted += OnInterrupted;
        }
    }

    private void OnDisable()
    {
        if (_caster != null)
        {
            _caster.OnCastAnimationRequired -= PlayAnimation;
            _caster.OnCastInterrupted -= OnInterrupted;
        }
    }

    private void PlayAnimation(string triggerName)
    {
        if (!string.IsNullOrEmpty(triggerName))
        {
            _animator.SetTrigger(triggerName);
        }
    }

    private void OnInterrupted()
    {
        _animator.SetTrigger("Interrupted");
    }

    public void OnAnimAttackPoint()
    {
        if (_caster != null) _caster.OnAnimAttackPoint();
    }

    public void OnAnimFinish()
    {
        if (_caster != null) _caster.OnAnimFinish();
    }
}