using UnityEngine;
using UnityEngine.Splines;

public class CarLaneController : MonoBehaviour
{
    [Header("차선 상태")]
    [SerializeField]
    private int currentLane = 0;

    [SerializeField]
    private float laneChangeSpeed = 2.5f;

    private float currentOffset;
    private float targetOffset;

    private CarPathFollower pathFollower;
    private SplineContainer lastPath;

    private int lastLaneCount = 1;

    private bool hasInitialLane = false;

    public int CurrentLane => currentLane;
    public float CurrentOffset => currentOffset;


    private void Awake()
    {
        pathFollower = GetComponent<CarPathFollower>();
    }


    private void Start()
    {
        lastPath = pathFollower != null ? pathFollower.CurrentPath : null;

        if (hasInitialLane) return;

        RefreshRoadLane(true);
    }


    private void Update()
    {
        if (pathFollower != null && pathFollower.CurrentPath != lastPath)
        {
            lastPath = pathFollower.CurrentPath;

            RefreshRoadLane(false);
        }

        currentOffset = Mathf.MoveTowards(currentOffset, targetOffset, laneChangeSpeed * Time.deltaTime);
    }


    public void RefreshRoadLane(bool isInitial = false)
    {
        if (pathFollower == null) return;

        RoadLaneData road = pathFollower.CurrentPath != null ? pathFollower.CurrentPath.GetComponent<RoadLaneData>() : null;

        int newLaneCount = road != null ? Mathf.Max(1, road.laneCount) : 1;


        // 1차선 도로로 들어옴
        if (newLaneCount == 1)
        {
            currentLane = 0;
            targetOffset = 0f;
        }
        else
        {
            if (lastLaneCount <= 1)
            {
                currentLane = Random.Range(0, newLaneCount);
            }
            else
            {
                currentLane = Mathf.Clamp(currentLane, 0, newLaneCount - 1);
            }

            targetOffset = CalculateLaneOffset(currentLane, road);
        }

        // 첫 시작만 즉시 Lane 기준으로 세팅
        if (isInitial)
        {
            currentOffset = targetOffset;
        }

        lastLaneCount = newLaneCount;
    }

    public void SetInitialLane(int laneIndex)
    {
        if (pathFollower == null)
        {
            pathFollower = GetComponent<CarPathFollower>();
        }

        if (pathFollower == null || pathFollower.CurrentPath == null)
        {
            Debug.LogWarning("[CarLaneController] " + "시작 차선보다 시작 도로를 먼저 지정해야 합니다.");

            return;
        }

        RoadLaneData road = pathFollower.CurrentPath.GetComponent<RoadLaneData>();

        if (road == null) return;

        int laneCount = Mathf.Max(1, road.laneCount);

        currentLane = Mathf.Clamp(laneIndex, 0, laneCount - 1);

        targetOffset = CalculateLaneOffset(currentLane, road);

        currentOffset = targetOffset;

        lastLaneCount = laneCount;
        lastPath = pathFollower.CurrentPath;

        hasInitialLane = true;
    }

    private float CalculateLaneOffset(int laneIndex, RoadLaneData road)
    {
        return (laneIndex - (road.laneCount - 1) * 0.5f) * road.laneWidth;
    }

    public void OnPathChanged(SplineContainer newPath)
    {
        lastPath = newPath;

        RefreshRoadLane(false);
    }
}