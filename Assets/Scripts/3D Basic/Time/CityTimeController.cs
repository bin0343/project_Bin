using UnityEngine;

public class CityTimeController : MonoBehaviour
{
    [Header("연결 요소")]
    public Light sunLight; // Directional Light 연결
    public Material skyboxMaterial; // 사용하는 Procedural Skybox Material 연결

    [Header("시간대별 설정값")]
    // 1. 아침 (Atmosphere: 1.8 ~ 2.0 / 해: 동쪽 낮은 곳)
    public TimeSetting morningSetting = new TimeSetting()
    {
        sunRotation = new Vector3(10f, 90f, 0f), // 동쪽에서 뜨는 해
        atmosphereThickness = 2.0f,
        sunColor = new Color(1f, 0.9f, 0.8f), // 따뜻한 노란색
        skyTint = new Color(0.8f, 0.7f, 0.6f),
        lightIntensity = 1.0f
    };

    // 2. 점심 (Atmosphere: 1.0 / 해: 남동쪽 높게 - 기존값 유지)
    public TimeSetting noonSetting = new TimeSetting()
    {
        sunRotation = new Vector3(41.339f, 135f, 0f), // 기존 설정값
        atmosphereThickness = 1.0f,
        sunColor = Color.white,
        skyTint = new Color(0.5f, 0.5f, 0.5f), // 기본 회색 (원색 유지)
        lightIntensity = 1.2f
    };

    // 3. 저녁 (Atmosphere: 0.5 / 해: 서쪽 낮은 곳 / 붉은 노을)
    public TimeSetting eveningSetting = new TimeSetting()
    {
        sunRotation = new Vector3(5f, 270f, 0f), // 서쪽으로 지는 해 (X를 낮게)
        atmosphereThickness = 0.5f, // 요청하신 값 (참고: 보통 노을은 1.5 이상이 붉게 나옴)
        sunColor = new Color(1f, 0.5f, 0.2f), // 주황색 빛
        skyTint = new Color(0.8f, 0.4f, 0.4f), // 붉은 틴트
        lightIntensity = 0.8f
    };

    // 4. 밤 (Atmosphere: 0.2 / 해: 달처럼 변신)
    public TimeSetting nightSetting = new TimeSetting()
    {
        sunRotation = new Vector3(144.338f, 34.658f, -48.36099f), // 반대편 하늘 높게 (달 위치)
        atmosphereThickness = 0.2f, // 요청하신 값 (우주처럼 맑게)
        sunColor = new Color(0.6f, 0.7f, 1.0f), // 푸르스름한 달빛
        skyTint = new Color(0.5f, 0.5f, 0.5f), // 어두운 남색
        lightIntensity = 0.3f // 어둡게
    };

    void Start()
    {
        // TimeManager가 없으면 에러 방지
        if (TimeManager.instance == null) return;

        // 현재 시간에 맞춰 환경 설정 적용
        ApplyTimeSetting(TimeManager.instance.currentTimeOfDay);
    }

    void ApplyTimeSetting(TimeOfDay time)
    {
        TimeSetting target = noonSetting; // 기본값

        switch (time)
        {
            case TimeOfDay.Morning: target = morningSetting; break;
            case TimeOfDay.Afternoon: target = noonSetting; break;
            case TimeOfDay.Evening: target = eveningSetting; break;
            case TimeOfDay.Night: target = nightSetting; break;
        }

        // 1. 해 위치/회전 변경
        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(target.sunRotation);
            sunLight.color = target.sunColor;
            sunLight.intensity = target.lightIntensity;
        }

        // 2. 스카이박스 변경 (Procedural Skybox 프로퍼티 수정)
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetFloat("_AtmosphereThickness", target.atmosphereThickness);
            skyboxMaterial.SetColor("_SkyTint", target.skyTint);

            // 밤에는 Exposure(노출)를 낮춰서 더 어둡게
            float exposure = (time == TimeOfDay.Night) ? 0.3f : 1.3f;
            skyboxMaterial.SetFloat("_Exposure", exposure);
        }

        // 3. 환경광(Ambient) 실시간 업데이트
        DynamicGI.UpdateEnvironment();
    }

    // 설정값 저장용 구조체
    [System.Serializable]
    public struct TimeSetting
    {
        public Vector3 sunRotation;
        public float atmosphereThickness;
        public Color sunColor;
        public Color skyTint;
        public float lightIntensity;
    }
}