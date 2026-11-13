using UnityEngine;

public class AICarAudio : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CarPathFollower pathFollower;
    [SerializeField] private CarSensor carSensor;

    [Header("엔진 AudioSource")]
    [SerializeField] private AudioSource idleSource;
    [SerializeField] private AudioSource highOnSource;
    [SerializeField] private AudioSource highOffSource;

    [Header("경적")]
    [SerializeField] private AudioSource hornSource;
    [SerializeField] private AudioClip hornClip;

    [Header("엔진 설정")]
    [SerializeField] private float stoppedSpeed = 0.3f;
    [SerializeField] private float audioFadeSpeed = 3f;
    [SerializeField] private float minDrivingPitch = 0.9f;
    [SerializeField] private float maxDrivingPitch = 1.1f;

    [Header("엔진 볼륨")]
    [SerializeField, Range(0f, 1f)]
    private float idleVolume = 0.2f;
    [SerializeField, Range(0f, 1f)]
    private float highOnVolume = 0.25f;
    [SerializeField, Range(0f, 1f)]
    private float highOffVolume = 0.2f;

    [Header("경적 설정")]
    [SerializeField] private float hornWaitTime = 2f;
    [SerializeField] private float hornCooldownMin = 4f;
    [SerializeField] private float hornCooldownMax = 7f;

    private float playerBlockTimer;
    private float nextHornTime;

    private void Awake()
    {
        if (pathFollower == null)
        {
            pathFollower = GetComponent<CarPathFollower>();
        }

        if (carSensor == null)
        {
            carSensor = GetComponent<CarSensor>();
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
        if (pathFollower == null) return;

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
        float speed = pathFollower.CurrentSpeed;
        float speedRatio = Mathf.InverseLerp(0f, pathFollower.CruiseSpeed, speed);
        bool isStopped = speed <= stoppedSpeed;
        bool isBraking = pathFollower.IsBraking;
        float idleTarget;
        float onTarget;
        float offTarget;

        if (isStopped)
        {
            idleTarget = idleVolume;
            onTarget = 0f;
            offTarget = 0f;
        }
        else if (isBraking)
        {
            idleTarget = 0f;
            onTarget = 0f;
            offTarget = highOffVolume;
        }
        else
        {
            idleTarget = 0f;
            onTarget = highOnVolume;
            offTarget = 0f;
        }

        FadeVolume(idleSource, idleTarget);
        FadeVolume(highOnSource, onTarget);
        FadeVolume(highOffSource, offTarget);

        float drivingPitch = Mathf.Lerp(minDrivingPitch, maxDrivingPitch, speedRatio);

        if (highOnSource != null)
        {
            highOnSource.pitch = drivingPitch;
        }

        if (highOffSource != null)
        {
            highOffSource.pitch =
                drivingPitch;
        }
    }

    private void FadeVolume(AudioSource source, float targetVolume)
    {
        if (source == null) return;

        source.volume = Mathf.MoveTowards(source.volume, targetVolume, audioFadeSpeed * Time.deltaTime);
    }

    private void UpdateHorn()
    {
        if (carSensor == null || hornSource == null || hornClip == null) return;


        bool playerBlocking = carSensor.IsPlayerBlocking;
        bool carStopped = pathFollower.CurrentSpeed <= stoppedSpeed;

        if (!playerBlocking || !carStopped)
        {
            playerBlockTimer = 0f;

            return;
        }

        playerBlockTimer += Time.deltaTime;

        if (playerBlockTimer < hornWaitTime) return;

        if (Time.time < nextHornTime) return;

        hornSource.PlayOneShot(hornClip);

        nextHornTime = Time.time + Random.Range(hornCooldownMin, hornCooldownMax);

        playerBlockTimer = 0f;
    }
}