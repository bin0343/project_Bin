using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class CarPathFollower : MonoBehaviour
{
    [Header("경로")]
    [SerializeField] private SplineContainer currentPath;

    [Header("속도")]
    [SerializeField] private float cruiseSpeed = 8f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float brakeDeceleration = 8f;
    [SerializeField] private float steerSpeed = 4f;

    [Header("경사 추종")]
    [SerializeField] private float heightFollowSpeed = 6f;

    [Header("경로 전환")]
    [SerializeField] private float pathSwitchDistance = 1.0f;
    [SerializeField, Range(0f, 1f)] private float pathSwitchMinT = 0.9f;

    private SplineContainer queuedPath;
    public SplineContainer CurrentPath => currentPath;
    public bool HasQueuedPath => queuedPath != null;

    private float splineHeightOffset;

    private float currentSpeed;
    private float targetSpeed;

    public float CurrentSpeed => currentSpeed;
    public float CruiseSpeed => cruiseSpeed;
    public float BrakeDeceleration => brakeDeceleration;
    public float TargetSpeed => targetSpeed;

    public bool IsBraking => targetSpeed < currentSpeed - 0.1f;
    

    [Header("경로 추종")]
    [SerializeField] private float lookAheadDistance = 5f;

    private Rigidbody rb;

    private void Start()
    {
        if (currentPath == null) return;

        if (FindNearestSplinePoint(out float nearestT, out Vector3 nearestPoint))
        {
            splineHeightOffset = rb.position.y - nearestPoint.y;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        currentSpeed = cruiseSpeed;
        targetSpeed = cruiseSpeed;
    }

    private void FixedUpdate()
    {
        if (currentPath == null) return;

        UpdateSpeed();

        if (!FindNearestSplinePoint(out float nearestT, out Vector3 nearestSplinePoint)) return;

        bool pathChanged = TrySwitchQueuedPath(nearestT);

        if (pathChanged)
        {
            if (!FindNearestSplinePoint(out nearestT, out nearestSplinePoint)) return;
        }

        Vector3 targetPoint = GetLookAheadPoint(nearestT);

        SteerToward(targetPoint);
        MoveForward(nearestSplinePoint);
    }

    public void SetTargetSpeed(float speed)
    {
        targetSpeed = Mathf.Clamp(speed, 0f, cruiseSpeed);
    }

    private void UpdateSpeed()
    {
        float changeRate;

        if (targetSpeed < currentSpeed)
        {
            changeRate = brakeDeceleration;
        }
        else
        {
            changeRate = acceleration;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, changeRate * Time.fixedDeltaTime);
    }

    private bool FindNearestSplinePoint(out float nearestT, out Vector3 nearestWorldPoint)
    {
        nearestT = 0f;
        nearestWorldPoint = rb.position;

        if (currentPath == null) return false;

        Vector3 localCarPosition = currentPath.transform.InverseTransformPoint(rb.position);

        SplineUtility.GetNearestPoint(currentPath.Spline, localCarPosition, out float3 nearestLocalPoint, out nearestT);

        nearestWorldPoint = currentPath.transform.TransformPoint((Vector3)nearestLocalPoint);

        return true;
    }

    private Vector3 GetLookAheadPoint(float nearestT)
    {
        float3 localTargetPoint = SplineUtility.GetPointAtLinearDistance(currentPath.Spline, nearestT, lookAheadDistance, out float targetT);

        return currentPath.transform.TransformPoint((Vector3)localTargetPoint);
    }

    private void SteerToward(Vector3 targetPoint)
    {
        Vector3 direction = targetPoint - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        Quaternion nextRotation = Quaternion.Slerp(rb.rotation, targetRotation, steerSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(nextRotation);
    }

    private void MoveForward(Vector3 nearestSplinePoint)
    {
        Vector3 nextPosition = rb.position + transform.forward * currentSpeed * Time.fixedDeltaTime;

        float targetY = nearestSplinePoint.y + splineHeightOffset;

        nextPosition.y = Mathf.MoveTowards(rb.position.y, targetY, heightFollowSpeed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);
    }

    //교차로
    public void QueueNextPath(SplineContainer nextPath)
    {
        if (nextPath == null) return;

        if (nextPath == currentPath) return;

        if (queuedPath != null) return;

        queuedPath = nextPath;
    }

    private bool TrySwitchQueuedPath(float nearestT)
    {
        if (queuedPath == null) return false;

        if (currentPath == null) return false;

        // 아직 Spline 후반부까지 오지 않았다면 전환하지 않음
        if (nearestT < pathSwitchMinT) return false;

        Vector3 endPoint = (Vector3)currentPath.EvaluatePosition(1f);

        float distanceToEnd = Vector3.Distance(rb.position, endPoint);

        if (distanceToEnd > pathSwitchDistance) return false;

        currentPath = queuedPath;
        queuedPath = null;

        return true;
    }
}
