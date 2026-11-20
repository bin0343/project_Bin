using UnityEngine;

public enum UISoundType
{
    Click,
    Hover,
    Open,
    Close,
    Confirm,
    Cancel
}

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField] private AudioClip cancelSound;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float clickVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float openVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float closeVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float confirmVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float cancelVolume = 0.8f;

    private void Awake()
    {
        Instance = this;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void Play(UISoundType type)
    {
        switch (type)
        {
            case UISoundType.Click:
                PlayOneShot(clickSound, clickVolume);
                break;
            case UISoundType.Hover:
                PlayOneShot(hoverSound, hoverVolume);
                break;
            case UISoundType.Open:
                PlayOneShot(openSound, openVolume);
                break;
            case UISoundType.Close:
                PlayOneShot(closeSound, closeVolume);
                break;
            case UISoundType.Confirm:
                PlayOneShot(confirmSound, confirmVolume);
                break;
            case UISoundType.Cancel:
                PlayOneShot(cancelSound, cancelVolume);
                break;
        }
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null) return;

        audioSource.PlayOneShot(clip, volume);
    }
}