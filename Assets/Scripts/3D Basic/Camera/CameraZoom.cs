using UnityEngine;
using Cinemachine;

public class CameraZoom : MonoBehaviour
{
    private CinemachineFreeLook freeLookCam;
    private CinemachineFreeLook.Orbit[] originalOrbits;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoomPercent = 0.5f; // 원래 크기의 50%까지 확대
    public float maxZoomPercent = 2.0f; // 원래 크기의 200%까지 축소

    [Header("Smooth Settings")]
    [Tooltip("값이 작을수록 더 부드럽고 미끄러지듯 움직입니다.")]
    public float smoothSpeed = 10f;

    private float targetZoomPercent = 1.0f;
    private float currentZoomPercent = 1.0f;

    void Start()
    {
        freeLookCam = GetComponent<CinemachineFreeLook>();

        // 인스펙터에 설정된 기본 탑, 미들, 바텀 링(Rig)의 값을 복사
        originalOrbits = new CinemachineFreeLook.Orbit[freeLookCam.m_Orbits.Length];
        for (int i = 0; i < originalOrbits.Length; i++)
        {
            originalOrbits[i] = freeLookCam.m_Orbits[i];
        }
    }

    void Update()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");

        if (scrollData != 0)
        {
            targetZoomPercent -= scrollData * zoomSpeed;
            targetZoomPercent = Mathf.Clamp(targetZoomPercent, minZoomPercent, maxZoomPercent);
        }

        currentZoomPercent = Mathf.Lerp(currentZoomPercent, targetZoomPercent, Time.deltaTime * smoothSpeed);

        for (int i = 0; i < freeLookCam.m_Orbits.Length; i++)
        {
            freeLookCam.m_Orbits[i].m_Height = originalOrbits[i].m_Height * currentZoomPercent;
            freeLookCam.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius * currentZoomPercent;
        }
    }
}