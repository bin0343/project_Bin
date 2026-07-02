using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance;

    [System.Serializable]
    public class ShakeSetting
    {
        [Tooltip("흔들림 세기")]
        public float amplitude = 0.7f;

        [Tooltip("흔들림 빠르기")]
        public float frequency = 14f;

        [Tooltip("흔들림 지속 시간")]
        public float duration = 0.1f;
    }

    [Header("Cinemachine")]
    [SerializeField] private CinemachineFreeLook freeLookCamera;

    [Header("Hit Shake Settings")]
    [Tooltip("1타/2타 같은 일반 공격이 적중했을 때의 쉐이크")]
    [SerializeField]
    private ShakeSetting normalHitShake = new ShakeSetting
    {
        amplitude = 0.7f,
        frequency = 14f,
        duration = 0.10f
    };

    [Tooltip("3타/넉백 공격이 적중했을 때의 쉐이크")]
    [SerializeField]
    private ShakeSetting knockbackHitShake = new ShakeSetting
    {
        amplitude = 1.6f,
        frequency = 18f,
        duration = 0.18f
    };

    private CinemachineBasicMultiChannelPerlin[] noises;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (freeLookCamera == null)
        {
            freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        }

        CacheNoiseComponents();
        SetNoises(0f, 0f);
    }

    private void Start()
    {
        CacheNoiseComponents();
        SetNoises(0f, 0f);
    }

    private void OnDisable()
    {
        SetNoises(0f, 0f);
    }

    private void CacheNoiseComponents()
    {
        if (freeLookCamera == null) return;

        noises = new CinemachineBasicMultiChannelPerlin[3];

        for (int i = 0; i < 3; i++)
        {
            CinemachineVirtualCamera rig = freeLookCamera.GetRig(i);
            if (rig == null) continue;

            noises[i] = rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void ShakeNormalHit()
    {
        Shake(normalHitShake);
    }

    public void ShakeKnockbackHit()
    {
        Shake(knockbackHitShake);
    }

    public void Shake(ShakeSetting setting)
    {
        if (setting == null) return;
        Shake(setting.amplitude, setting.frequency, setting.duration);
    }

    public void Shake(float amplitude, float frequency, float duration)
    {
        if (freeLookCamera == null)
        {
            freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        }

        if (noises == null || noises.Length == 0)
        {
            CacheNoiseComponents();
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(amplitude, frequency, duration));
    }

    private IEnumerator ShakeRoutine(float amplitude, float frequency, float duration)
    {
        if (duration <= 0f)
        {
            SetNoises(0f, 0f);
            shakeCoroutine = null;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float power = Mathf.Lerp(1f, 0f, t);

            SetNoises(amplitude * power, frequency);

            yield return null;
        }

        SetNoises(0f, 0f);
        shakeCoroutine = null;
    }

    private void SetNoises(float amplitude, float frequency)
    {
        if (noises == null) return;

        for (int i = 0; i < noises.Length; i++)
        {
            if (noises[i] == null) continue;

            noises[i].m_AmplitudeGain = amplitude;
            noises[i].m_FrequencyGain = frequency;
        }
    }
}
