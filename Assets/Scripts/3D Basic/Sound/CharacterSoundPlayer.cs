using UnityEngine;

public class CharacterSoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Movement")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.5f;

    [SerializeField] private AudioClip jumpSound;
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.7f;

    [SerializeField] private AudioClip landSound;
    [SerializeField, Range(0f, 1f)] private float landVolume = 0.7f;

    [SerializeField] private AudioClip dodgeSound;
    [SerializeField, Range(0f, 1f)] private float dodgeVolume = 0.7f;

    [Header("Combat")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField, Range(0f, 1f)] private float attackVolume = 0.8f;

    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 0.8f;

    [SerializeField] private AudioClip deathSound;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;

    private bool footstepEnabled = true;

    public void SetFootstepEnabled(bool enabled)
    {
        footstepEnabled = enabled;
    }


    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }


    public void PlayFootstep()
    {
        if (!footstepEnabled) return;

        PlayRandom(footstepSounds, footstepVolume);
    }

    public void PlayJump()
    {
        PlayOneShot(jumpSound, jumpVolume);
    }

    public void PlayLand()
    {
        PlayOneShot(landSound, landVolume);
    }

    public void PlayDodge()
    {
        PlayOneShot(dodgeSound, dodgeVolume);
    }

    public void PlayAttack(int attackIndex)
    {
        if (attackSounds == null || attackSounds.Length == 0) return;

        int index = attackIndex - 1;

        if (index < 0 || index >= attackSounds.Length) return;

        PlayOneShot(attackSounds[index], attackVolume);
    }

    public void PlaySkill(AudioClip clip, float volume)
    {
        PlayOneShot(clip, volume);
    }

    public void PlayHit()
    {
        PlayRandom(hitSounds, hitVolume);
    }

    public void PlayDeath()
    {
        PlayOneShot(deathSound, deathVolume);
    }


    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null) return;

        audioSource.PlayOneShot(clip, volume);
    }


    private void PlayRandom(AudioClip[] clips, float volume)
    {
        if (audioSource == null) return;

        if (clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);

        audioSource.PlayOneShot(clips[index], volume);
    }
}