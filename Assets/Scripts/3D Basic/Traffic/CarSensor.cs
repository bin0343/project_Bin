using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class CarSensor : MonoBehaviour
{
    [Header("레이더 설정")]
    public float sensorLength = 12f;
    public float sensorRadius = 0.5f;
    public float stopOffset = 1.0f;
    public float passYellowDistance = 4f;

    public LayerMask obstacleLayer;
    public LayerMask playerLayer;

    [Header("차량 추종(ACC) 설정")]
    public float safeDistance = 6f;

    [Header("가속 설정")]
    public float accelDuration = 1.5f;

    private SplineAnimate carAnim;
    public bool isBraking = false;
    public bool isAccelerating = false;

    public bool isInsideIntersection = false;   //교차로 통과 중인지

    private float originalSpeed;
    public float currentSpeed;
    private TrafficLightController currentTargetLight;
    private Transform carMesh;

    void Start()
    {
        carAnim = GetComponent<SplineAnimate>();
        carMesh = transform.Find("Car_Model");
        originalSpeed = carAnim.MaxSpeed;
        currentSpeed = originalSpeed;
        carAnim.Pause();
    }

    void Update()
    {
        if (carAnim.IsPlaying) carAnim.Pause();

        Vector3 sensorStart = carMesh.position + (transform.forward * 2f) + (Vector3.up * 0.5f);
        RaycastHit hit;

        LayerMask combinedMask = obstacleLayer | playerLayer;

        bool hasHit = Physics.SphereCast(sensorStart, sensorRadius, transform.forward, out hit, sensorLength, combinedMask);

        bool isFollowingCar = false;
        bool shouldBrakeForCar = false;
        float carBrakeDistance = 0f;

        //플레이어 감지
        bool shouldBrakeForPlayer = false;
        float playerBrakeDistance = 0f;

        //양보 센서(병목, 교차로 충돌 방지)
        bool shouldYield = false;
        float yieldDistance = 0f;

        Vector3 yieldSensorPos = carMesh.position + (transform.forward * 1.5f) + (Vector3.up * 0.5f);
        // 반경 1.8m의 구체를 생성하여 주변 차를 감지 (정상 차간 거리인 3m 밖의 옆 차는 무시)
        Collider[] nearbyCars = Physics.OverlapSphere(yieldSensorPos, 1.8f, combinedMask);

        foreach (var col in nearbyCars)
        {
            if (col.CompareTag("Car") && col.transform != this.carMesh && col.transform.parent != this.transform)
            {
                CarSensor otherCar = col.GetComponentInParent<CarSensor>();
                if (otherCar != null)
                {
                    Vector3 toOther = col.transform.position - carMesh.position;

                    // 좌우로 0.5m 이상 떨어져 있는 '옆차/끼어드는 차'만 양보 대상으로 지정
                    float lateralDist = Mathf.Abs(Vector3.Dot(transform.right, toOther));

                    if (lateralDist > 0.5f)
                    {
                        float forwardDot = Vector3.Dot(transform.forward, toOther.normalized);

                        // 옆차가 나보다 살짝 앞이거나 교차로 대각선에서 들어올 때
                        if (forwardDot > 0.1f)
                        {
                            shouldYield = true;
                            yieldDistance = toOther.magnitude;
                        }
                        // 병목 구간에서 완벽히 나란히 달리고 있을 때 (데드락 방지)
                        else if (Mathf.Abs(forwardDot) <= 0.1f)
                        {
                            // 고유 ID가 더 작은 녀석이 억울하게 브레이크를 밟습니다. (절대 둘이 겹치지 않음!)
                            if (this.gameObject.GetInstanceID() < otherCar.gameObject.GetInstanceID())
                            {
                                shouldYield = true;
                                yieldDistance = 1.0f;
                            }
                        }
                    }
                }
            }
        }

        //전방 감지 시스템
        if (hasHit)
        {
            if (hit.collider.CompareTag("Player"))
            {
                shouldBrakeForPlayer = true;
                playerBrakeDistance = hit.distance;
            }
            if (hit.collider.CompareTag("StopLine") && !isInsideIntersection)
            {
                currentTargetLight = hit.collider.GetComponentInParent<TrafficLightController>();
            }
            else if (hit.collider.CompareTag("Car"))
            {
                CarSensor frontCar = hit.collider.GetComponentInParent<CarSensor>();

                if (frontCar != null && frontCar.gameObject != this.gameObject)
                {
                    if (hit.distance < safeDistance)
                    {
                        if (frontCar.currentSpeed < 0.5f)
                        {
                            shouldBrakeForCar = true;
                            carBrakeDistance = hit.distance;
                        }
                        else
                        {
                            isFollowingCar = true;

                            // 크루즈 컨트롤 중이면 브레이크/엑셀 상태 모두 해제
                            if (isBraking || isAccelerating)
                            {
                                isBraking = false;
                                isAccelerating = false;
                                DOTween.Kill(this);
                            }

                            currentSpeed = Mathf.Lerp(currentSpeed, frontCar.currentSpeed, Time.deltaTime * 5f);
                        }
                    }
                }
            }
        }
        else
        {
            if (!isBraking) currentTargetLight = null;
        }

        if (shouldBrakeForPlayer)
        {
            if (!isBraking && currentSpeed > 0.1f) ApplyBrake(playerBrakeDistance);
        }
        else if (shouldBrakeForCar)
        {
            if (!isBraking && currentSpeed > 0.1f) ApplyBrake(carBrakeDistance);
        }
        else if (shouldYield)
        {
            if (!isBraking && currentSpeed > 0.1f) ApplyBrake(yieldDistance);
        }
        else if (currentTargetLight != null)
        {
            if (currentTargetLight.currentState == TrafficLightController.LightState.Red)
            {
                if (!isBraking && currentSpeed > 0.1f) ApplyBrake(hit.distance);
            }
            else if (currentTargetLight.currentState == TrafficLightController.LightState.Yellow)
            {
                if (hit.distance <= passYellowDistance)
                {
                    currentTargetLight = null;
                    if (!isFollowingCar) ApplyThrottle();
                }
                else
                {
                    if (!isBraking && currentSpeed > 0.1f) ApplyBrake(hit.distance);
                }
            }
            else if (currentTargetLight.currentState == TrafficLightController.LightState.Green)
            {
                currentTargetLight = null;
                if (!isFollowingCar) ApplyThrottle();
            }
        }
        else
        {
            if (!isFollowingCar)
            {
                if (isBraking || currentSpeed < originalSpeed - 0.1f) ApplyThrottle();
            }
        }

        // 바퀴 굴리기
        if (carAnim.Container != null)
        {
            float splineLength = carAnim.Container.CalculateLength();
            if (splineLength > 0f)
            {
                float moveDistance = currentSpeed * Time.deltaTime;
                carAnim.NormalizedTime += moveDistance / splineLength;
                if (carAnim.NormalizedTime > 1f) carAnim.NormalizedTime = 1f;
            }
        }
    }

    void ApplyBrake(float distanceToCube)
    {
        isBraking = true;
        isAccelerating = false; //브레이크를 밟으면 엑셀 상태 해제
        DOTween.Kill(this);

        float distanceToStop = distanceToCube - stopOffset;
        if (distanceToStop < 0.1f) distanceToStop = 0.1f;

        float calcSpeed = currentSpeed;
        if (calcSpeed < 0.1f) calcSpeed = 0.1f;

        float calculatedBrakeTime = (distanceToStop * 2f) / calcSpeed;
        calculatedBrakeTime = Mathf.Clamp(calculatedBrakeTime, 0.1f, 4f);

        DOTween.To(() => currentSpeed, x => currentSpeed = x, 0f, calculatedBrakeTime)
            .SetEase(Ease.Linear)
            .SetId(this)
            .OnComplete(() => {
                currentSpeed = 0f;
            });
    }

    void ApplyThrottle()
    {
        if (isAccelerating) return;

        isBraking = false;
        isAccelerating = true; // 엑셀 밟기 시작
        DOTween.Kill(this);

        DOTween.To(() => currentSpeed, x => currentSpeed = x, originalSpeed, accelDuration)
            .SetEase(Ease.InQuad)
            .SetId(this)
            .OnComplete(() => { isAccelerating = false; }); //최고 속도 도달 시 상태 해제
    }
}