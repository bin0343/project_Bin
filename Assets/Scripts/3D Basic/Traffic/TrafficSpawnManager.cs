using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TrafficSpawnManager : MonoBehaviour
{
    [Header("AI 차량 프리팹")]
    [SerializeField] private GameObject[] trafficVehiclePrefabs;

    [Header("초기 배치")]
    [Min(0)]
    [SerializeField] private int initialVehicleCount = 10;

    [SerializeField, Range(0f, 1f)]
    private float spawnMinT = 0.08f;

    [SerializeField, Range(0f, 1f)]
    private float spawnMaxT = 0.92f;

    [Tooltip("차량 한 대를 배치할 때 빈 자리를 찾는 최대 시도 횟수")]
    [Min(1)]
    [SerializeField] private int maxSpawnAttempts = 20;

    [Header("배치 공간 검사")]
    [SerializeField] private LayerMask vehicleLayer;

    [Tooltip("차량 생성 위치 주변에 확보할 공간")]
    [SerializeField] private Vector3 spawnCheckHalfExtents = new Vector3(1.5f, 1f, 4f);

    private readonly List<RoadLaneData> spawnableRoads = new List<RoadLaneData>();

    private void Start()
    {
        CollectSpawnableRoads();

        SpawnInitialTraffic();
    }

    private void CollectSpawnableRoads()
    {
        spawnableRoads.Clear();

        RoadLaneData[] roads = FindObjectsOfType<RoadLaneData>();

        foreach (RoadLaneData road in roads)
        {
            if (road == null) continue;
            if (!road.allowTrafficSpawn) continue;

            SplineContainer spline = road.GetComponent<SplineContainer>();

            if (spline == null) continue;

            spawnableRoads.Add(road);
        }
    }

    private void SpawnInitialTraffic()
    {
        if (trafficVehiclePrefabs == null || trafficVehiclePrefabs.Length == 0)
        {
            Debug.LogWarning("[TrafficSpawn] AI 차량 프리팹이 없습니다.");

            return;
        }

        if (spawnableRoads.Count == 0)
        {
            Debug.LogWarning("[TrafficSpawn] 배치 가능한 도로가 없습니다.");

            return;
        }

        int spawnedCount = 0;

        for (int i = 0; i < initialVehicleCount; i++)
        {
            if (TrySpawnVehicle())
            {
                spawnedCount++;
            }
        }

        Debug.Log($"[TrafficSpawn] 초기 차량 배치 완료 " + $"{spawnedCount}/{initialVehicleCount}");
    }

    private bool TrySpawnVehicle()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            RoadLaneData road = spawnableRoads[Random.Range(0, spawnableRoads.Count)];

            SplineContainer spline = road.GetComponent<SplineContainer>();

            if (spline == null) continue;

            int laneIndex = Random.Range(0, Mathf.Max(1, road.laneCount));

            float t = Random.Range(spawnMinT, spawnMaxT);

            if (!TryGetSpawnPose(road, spline, laneIndex, t, out Vector3 position, out Quaternion rotation)) continue;

            if (!IsSpawnAreaClear(position, rotation)) continue;

            SpawnVehicle(road, spline, laneIndex, position, rotation);

            return true;
        }

        return false;
    }

    private bool TryGetSpawnPose(RoadLaneData road, SplineContainer spline, int laneIndex, float t, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        Vector3 splinePosition = (Vector3)spline.EvaluatePosition(t);
        Vector3 tangent = (Vector3)spline.EvaluateTangent(t);

        tangent.y = 0f;

        if (tangent.sqrMagnitude < 0.001f) return false;

        tangent.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

        float laneOffset = CalculateLaneOffset(laneIndex, road);

        position = splinePosition + right * laneOffset;
        rotation = Quaternion.LookRotation(tangent, Vector3.up);

        return true;
    }

    private float CalculateLaneOffset(int laneIndex, RoadLaneData road)
    {
        return (laneIndex - (road.laneCount - 1) * 0.5f) * road.laneWidth;
    }

    private bool IsSpawnAreaClear(Vector3 position, Quaternion rotation)
    {
        Vector3 checkCenter = position + Vector3.up * spawnCheckHalfExtents.y;

        bool blocked = Physics.CheckBox(checkCenter, spawnCheckHalfExtents, rotation, vehicleLayer, QueryTriggerInteraction.Ignore);

        return !blocked;
    }

    private void SpawnVehicle(RoadLaneData road, SplineContainer spline, int laneIndex, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = trafficVehiclePrefabs[Random.Range(0, trafficVehiclePrefabs.Length)];
        GameObject vehicle = Instantiate(prefab, position, rotation);

        CarPathFollower pathFollower = vehicle.GetComponent<CarPathFollower>();
        CarLaneController laneController = vehicle.GetComponent<CarLaneController>();

        if (pathFollower == null || laneController == null)
        {
            Debug.LogWarning("[TrafficSpawn] 생성된 차량에 " + "필수 AI 컴포넌트가 없습니다.");

            Destroy(vehicle);

            return;
        }

        pathFollower.SetInitialPath(spline);

        laneController.SetInitialLane(laneIndex);
    }
}