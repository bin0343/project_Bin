using UnityEngine;
using UnityEngine.Splines;

public class CarSensor : MonoBehaviour
{
    [Header("레이더 설정")]
    public float sensorLength = 5f;
    public LayerMask obstacleLayer;

    private SplineAnimate carAnim;
    private bool isWaitingForLight = false;

    void Start()
    {
        carAnim = GetComponent<SplineAnimate>();
    }

    void Update()
    {
        Vector3 sensorStart = transform.position + (transform.forward * 2f) + (Vector3.up * 0.5f);
        RaycastHit hit;

        Debug.DrawRay(sensorStart, transform.forward * sensorLength, Color.red);

        // 레이저가 지정된 레이어(obstacleLayer)에 맞았을 때
        if (Physics.Raycast(sensorStart, transform.forward, out hit, sensorLength, obstacleLayer))
        {
            if (hit.collider.CompareTag("StopLine"))
            {
                TrafficLightController light = hit.collider.GetComponentInParent<TrafficLightController>();

                if (light != null && light.currentState == TrafficLightController.LightState.Red)
                {
                    carAnim.Pause();
                    isWaitingForLight = true;
                }
                else if (light != null && light.currentState == TrafficLightController.LightState.Green)
                {
                    if (isWaitingForLight)
                    {
                        carAnim.Play();
                        isWaitingForLight = false;
                    }
                }
            }
        }
        else
        {
            // 레이저에 아무것도 안 걸리는데 대기 중이었다면 출발 (앞차가 빠졌을 때 등)
            if (isWaitingForLight)
            {
                carAnim.Play();
                isWaitingForLight = false;
            }
        }
    }
}