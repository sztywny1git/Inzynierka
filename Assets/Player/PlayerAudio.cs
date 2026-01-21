using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(IHealthProvider))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepVolume = 0.5f;

    [Header("Combat Actions")]
    [SerializeField] private AudioClip[] attackEffortSounds;
    [SerializeField] private float attackVolume = 1.0f;

    [Header("Health Events")]
    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private float damageVolume = 1.0f;

    private AudioSource _audioSource;
    private IHealthProvider _healthProvider;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _healthProvider = GetComponent<IHealthProvider>();
    }

    private void OnEnable()
    {
        if (_healthProvider != null)
        {
            _healthProvider.OnDamageTaken += PlayHurtSound;
            _healthProvider.Death += PlayDeathSound;
        }
    }

    private void OnDisable()
    {
        if (_healthProvider != null)
        {
            _healthProvider.OnDamageTaken -= PlayHurtSound;
            _healthProvider.Death -= PlayDeathSound;
        }
    }

    private void PlayClip(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0) return;

        var clip = clips[Random.Range(0, clips.Length)];
        _audioSource.PlayOneShot(clip, volume);
    }

    private void PlayHurtSound(DamageData data)
    {
        PlayClip(hurtSounds, damageVolume);
    }

    private void PlayDeathSound()
    {
        PlayClip(deathSounds, damageVolume);
    }

    public void OnAnimFootstep()
    {
        PlayClip(footstepSounds, footstepVolume);
    }

    public void OnAnimAttackSound()
    {
        PlayClip(attackEffortSounds, attackVolume);
    }
}