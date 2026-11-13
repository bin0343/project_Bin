using UnityEngine;

public class CharacterSoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Movement")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip dodgeSound;

    [Header("Combat")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;


    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }


    public void PlayFootstep()
    {
        PlayRandom(footstepSounds);
    }


    public void PlayJump()
    {
        PlayOneShot(jumpSound);
    }


    public void PlayLand()
    {
        PlayOneShot(landSound);
    }


    public void PlayDodge()
    {
        PlayOneShot(dodgeSound);
    }


    public void PlayAttack(int attackIndex)
    {
        if (attackSounds == null || attackSounds.Length == 0) return;

        int index = attackIndex - 1;

        if (index < 0 || index >= attackSounds.Length) return;

        PlayOneShot(attackSounds[index]);
    }


    public void PlayHit()
    {
        PlayOneShot(hitSound);
    }


    public void PlayDeath()
    {
        PlayOneShot(deathSound);
    }


    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.PlayOneShot(clip);
    }


    private void PlayRandom(AudioClip[] clips)
    {
        if (audioSource == null) return;

        if (clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);

        audioSource.PlayOneShot(clips[index]);
    }
}