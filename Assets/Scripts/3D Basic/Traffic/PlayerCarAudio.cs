using UnityEngine;

public class PlayerCarAudio : MonoBehaviour
{
    [Header("차량")]
    [SerializeField] private PlayerVehicleController controller;

    [Header("엔진 AudioSource")]
    [SerializeField] private AudioSource idleSource;
    [SerializeField] private AudioSource highOnSource;
    [SerializeField] private AudioSource highOffSource;

    [Header("경적")]
    [SerializeField] private AudioSource hornSource;
    [SerializeField] private AudioClip hornClip;
    [SerializeField] private KeyCode hornKey = KeyCode.H;

    [Header("엔진 볼륨")]
    [SerializeField, Range(0f, 1f)]
    private float idleVolume = 0.03f;
    [SerializeField, Range(0f, 1f)]
    private float highOnVolume = 0.035f;
    [SerializeField, Range(0f, 1f)]
    private float highOffVolume = 0.03f;

    [Header("엔진 설정")]
    [SerializeField] private float stoppedSpeed = 0.3f;
    [SerializeField] private float audioFadeSpeed = 3f;
    [SerializeField] private float minDrivingPitch = 0.9f;
    [SerializeField] private float maxDrivingPitch = 1.1f;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerVehicleController>();
        }
    }


    private void Start()
    {
        StartEngineSource(idleSource);
        StartEngineSource(highOnSource);
        StartEngineSource(highOffSource);
    }


    private void Update()
    {
        if (controller == null) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(
                $"[PlayerCarAudio] " +
                $"IsDriving = {controller.IsDriving}, " +
                $"Speed = {controller.ForwardSpeed:F2}, " +
                $"IdlePlaying = {idleSource.isPlaying}, " +
                $"IdleVolume = {idleSource.volume:F3}"
            );
        }

        UpdateEngineAudio();
        UpdateHorn();
    }


    private void StartEngineSource(AudioSource source)
    {
        if (source == null) return;

        source.loop = true;
        source.volume = 0f;

        source.Play();
    }

    private void UpdateEngineAudio()
    {
        // 차량에 탑승하지 않은 상태
        if (!controller.IsDriving)
        {
            FadeVolume(idleSource, 0f);
            FadeVolume(highOnSource, 0f);
            FadeVolume(highOffSource, 0f);

            return;
        }

        float speed = Mathf.Abs(controller.ForwardSpeed);

        bool isStopped = speed <= stoppedSpeed;
        bool hasThrottleInput = Mathf.Abs(controller.MoveInput) > 0.01f;
        bool isBraking = controller.BrakeInput;

        float idleTarget = 0f;
        float onTarget = 0f;
        float offTarget = 0f;

        if (isStopped)
        {
            idleTarget = idleVolume;
        }
        else if (isBraking || !hasThrottleInput)
        {
            offTarget = highOffVolume;
        }
        else
        {
            onTarget = highOnVolume;
        }

        FadeVolume(idleSource, idleTarget);
        FadeVolume(highOnSource, onTarget);
        FadeVolume(highOffSource, offTarget);

        float maxSpeed = controller.ForwardSpeed >= 0f ? controller.MaxForwardSpeed : controller.MaxReverseSpeed;
        float speedRatio = Mathf.InverseLerp(0f, maxSpeed, speed);
        float drivingPitch = Mathf.Lerp(minDrivingPitch, maxDrivingPitch, speedRatio);

        if (highOnSource != null)
        {
            highOnSource.pitch = drivingPitch;
        }

        if (highOffSource != null)
        {
            highOffSource.pitch = drivingPitch;
        }
    }

    private void FadeVolume(AudioSource source, float targetVolume)
    {
        if (source == null) return;

        source.volume = Mathf.MoveTowards(source.volume, targetVolume, audioFadeSpeed * Time.deltaTime);
    }

    private void UpdateHorn()
    {
        if (!controller.IsDriving) return;

        if (hornSource == null || hornClip == null) return;

        if (Input.GetKeyDown(hornKey))
        {
            hornSource.PlayOneShot(hornClip);
        }
    }
}