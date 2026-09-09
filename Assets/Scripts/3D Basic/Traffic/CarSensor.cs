using UnityEngine;

public class CarSensor : MonoBehaviour
{
    [Header("플레이어 감지")]
    [SerializeField] private float playerSensorWidth = 2.6f;
    [SerializeField] private float playerSensorHeight = 1.8f;

    [Tooltip("차량 중심에서 감지 박스가 시작되는 위치")]
    [SerializeField] private float playerSensorForwardOffset = 0.5f;

    [Tooltip("플레이어가 순간적으로 감지에서 빠져도 바로 출발하지 않도록 유지하는 시간")]
    [SerializeField] private float playerReleaseDelay = 0.5f;

    private bool isPlayerBlocking = false;
    private float playerClearTimer = 0f;
    private readonly Collider[] playerDetectResults = new Collider[16];

    [Header("전방 센서")]
    public float sensorLength = 12f;
    public float sensorRadius = 0.5f;

    [Tooltip("앞차와 완전히 정지했을 때 확보할 최소 거리")]
    public float stopOffset = 1.0f;

    [Tooltip("노란불일 때 이 거리 안이라면 그냥 통과")]
    public float passYellowDistance = 4f;

    [Tooltip("빨간불일 때 정지선 앞에 남길 거리")]
    [SerializeField] private float trafficLightStopOffset = 0.5f;

    public LayerMask obstacleLayer;
    public LayerMask playerLayer;

    [Header("차량 추종")]
    public float safeDistance = 6f;

    [Header("교차로 정차")]
    [SerializeField] private float intersectionStopOffset = 0.5f;

    private CarPathFollower pathFollower;
    private Transform carMesh;

    public float currentSpeed => pathFollower != null ? pathFollower.CurrentSpeed : 0f;

    public bool isBraking => pathFollower != null && pathFollower.IsBraking;
    public bool isInsideIntersection = false;
    private bool isIntersectionBlocked = false;
    private bool hasIntersectionCommitment = false;
    private Vector3 intersectionStopPosition;
    private TrafficLightController currentTargetLight;
    private float currentStopLineDistance = float.MaxValue;

    public void SetIntersectionBlocked(bool blocked, Vector3 stopPosition)
    {
        isIntersectionBlocked = blocked;

        if (blocked)
        {
            intersectionStopPosition = stopPosition;
        }
    }

    public void SetIntersectionCommitment(bool committed)
    {
        hasIntersectionCommitment = committed;

        if (committed)
        {
            currentTargetLight = null;
            currentStopLineDistance = float.MaxValue;
        }
    }

    private void Start()
    {
        pathFollower = GetComponent<CarPathFollower>();
        carMesh = transform.Find("Car_Model");
    }

    private void Update()
    {
        if (pathFollower == null || carMesh == null)
            return;

        float desiredSpeed = pathFollower.CruiseSpeed;

        UpdatePlayerDetection();

        if (isPlayerBlocking)
        {
            desiredSpeed = 0f;
        }

        Vector3 sensorStart = carMesh.position + transform.forward * 2f + Vector3.up * 0.5f;

        bool hasHit = Physics.SphereCast(sensorStart, sensorRadius, transform.forward, out RaycastHit hit, sensorLength, obstacleLayer, QueryTriggerInteraction.Collide);

        if (hasHit)
        {
            //신호등 StopLine
            if (hit.collider.CompareTag("StopLine") && !isInsideIntersection && !hasIntersectionCommitment)
            {
                TrafficLightController detectedLight = hit.collider.GetComponentInParent<TrafficLightController>();

                if (detectedLight != null)
                {
                    currentTargetLight = detectedLight;
                    currentStopLineDistance = hit.distance;
                }
            }
            //앞차
            else if (hit.collider.CompareTag("Car"))
            {
                CarSensor frontCar = hit.collider.GetComponentInParent<CarSensor>();

                if (frontCar != null && frontCar.gameObject != gameObject && hit.distance < safeDistance)
                {
                    bool frontCarIsStopping = frontCar.isBraking || frontCar.currentSpeed < 0.5f;

                    if (frontCarIsStopping)
                    {
                        float remainingStopDistance = Mathf.Max(hit.distance - stopOffset, 0f);

                        float brakingSpeed = Mathf.Sqrt(2f * pathFollower.BrakeDeceleration * remainingStopDistance);

                        desiredSpeed = Mathf.Min(desiredSpeed, brakingSpeed);
                    }
                    else
                    {
                        float distanceRatio = Mathf.InverseLerp(stopOffset, safeDistance, hit.distance);

                        float distanceLimitedSpeed = pathFollower.CruiseSpeed * distanceRatio;

                        desiredSpeed = Mathf.Min(desiredSpeed, frontCar.currentSpeed, distanceLimitedSpeed);
                    }
                }
            }
        }
        else
        {
            currentTargetLight = null;
            currentStopLineDistance = float.MaxValue;
        }

        //신호 판단
        if (currentTargetLight != null && !isInsideIntersection && !hasIntersectionCommitment)
        {
            switch (currentTargetLight.currentState)
            {
                case TrafficLightController.LightState.Red:
                    {
                        float brakingSpeed = CalculateBrakingSpeed(currentStopLineDistance, trafficLightStopOffset);
                        desiredSpeed = Mathf.Min(desiredSpeed, brakingSpeed);
                        break;
                    }

                case TrafficLightController.LightState.Yellow:
                    {
                        if (currentStopLineDistance <= passYellowDistance)
                        {
                            break;
                        }

                        float brakingSpeed = CalculateBrakingSpeed(currentStopLineDistance, trafficLightStopOffset);
                        desiredSpeed = Mathf.Min(desiredSpeed, brakingSpeed);
                        break;
                    }

                case TrafficLightController.LightState.Green:
                    {
                        currentTargetLight = null;
                        currentStopLineDistance = float.MaxValue;
                        break;
                    }
            }
        }

        if (isIntersectionBlocked)
        {
            Vector3 sensorPosition = carMesh.position + transform.forward * 2f;

            Vector3 toStopPoint = intersectionStopPosition - sensorPosition;

            float distanceToStopPoint = Vector3.Dot(transform.forward, toStopPoint);

            distanceToStopPoint = Mathf.Max(distanceToStopPoint, 0f);

            float brakingSpeed = CalculateBrakingSpeed(distanceToStopPoint, intersectionStopOffset);

            desiredSpeed = Mathf.Min(desiredSpeed, brakingSpeed);
        }

        pathFollower.SetTargetSpeed(desiredSpeed);
    }

    public bool CanProceedThroughSignal(TrafficLightController light, Vector3 stopPosition)
    {
        if (light == null) return true;

        switch (light.currentState)
        {
            case TrafficLightController.LightState.Green:
                return true;
            case TrafficLightController.LightState.Red:
                return false;
            case TrafficLightController.LightState.Yellow:
                {
                    if (carMesh == null)
                        return false;

                    Vector3 toStopLine = stopPosition - carMesh.position;

                    float distance = Vector3.Dot(transform.forward, toStopLine);

                    return distance <= passYellowDistance;
                }
        }

        return false;
    }

    #region Player Detection

    private bool DetectPlayerInFront(out float nearestDistance)
    {
        nearestDistance = float.MaxValue;

        Vector3 boxStart = carMesh.position + transform.forward * playerSensorForwardOffset;

        Vector3 boxCenter = boxStart + transform.forward * (sensorLength * 0.5f) + Vector3.up * (playerSensorHeight * 0.5f);

        Vector3 halfExtents = new Vector3(playerSensorWidth * 0.5f, playerSensorHeight * 0.5f, sensorLength * 0.5f);

        int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, playerDetectResults, transform.rotation, playerLayer, QueryTriggerInteraction.Ignore);

        bool foundPlayer = false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = playerDetectResults[i];

            if (col == null) continue;

            Player_Action player = col.GetComponentInParent<Player_Action>();

            if (player == null) continue;

            Vector3 closestPoint = col.ClosestPoint(carMesh.position);

            Vector3 toPlayer = closestPoint - carMesh.position;

            float forwardDistance = Vector3.Dot(transform.forward, toPlayer);

            forwardDistance = Mathf.Max(forwardDistance, 0.1f);

            if (forwardDistance < nearestDistance)
            {
                nearestDistance = forwardDistance;
            }

            foundPlayer = true;
        }

        return foundPlayer;
    }

    private void UpdatePlayerDetection()
    {
        bool detected = DetectPlayerInFront(out float distance);

        if (detected)
        {
            isPlayerBlocking = true;
            playerClearTimer = 0f;
        }
        else if (isPlayerBlocking)
        {
            playerClearTimer += Time.deltaTime;

            if (playerClearTimer >= playerReleaseDelay)
            {
                isPlayerBlocking = false;
                playerClearTimer = 0f;
            }
        }
    }

    #endregion

    private float CalculateBrakingSpeed(float distance, float stopDistance)
    {
        float remainingDistance = Mathf.Max(distance - stopDistance, 0f);

        return Mathf.Sqrt(2f * pathFollower.BrakeDeceleration * remainingDistance);
    }

    #region Debug

    private void OnDrawGizmosSelected()
    {
        Transform mesh =
            transform.Find("Car_Model");

        if (mesh == null)
            return;

        Vector3 boxStart = mesh.position + transform.forward * playerSensorForwardOffset;

        Vector3 boxCenter = boxStart + transform.forward * (sensorLength * 0.5f) + Vector3.up * (playerSensorHeight * 0.5f);

        Vector3 boxSize = new Vector3(playerSensorWidth, playerSensorHeight, sensorLength);

        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Gizmos.matrix = Matrix4x4.identity;

        Vector3 sensorStart = mesh.position + transform.forward * 2f + Vector3.up * 0.5f;

        Gizmos.DrawWireSphere(sensorStart, sensorRadius);

        Gizmos.DrawLine(sensorStart, sensorStart + transform.forward * sensorLength);
    }

    #endregion
}