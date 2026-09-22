using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class VehicleSummonManager : MonoBehaviour
{
    [Header("소환 설정")]
    [SerializeField] private GameObject vehiclePrefab;
    [SerializeField] private KeyCode summonKey = KeyCode.G;

    [Tooltip("플레이어와 도로 사이 최대 소환 거리")]
    [SerializeField] private float maxSummonDistance = 12f;

    [Tooltip("Spline 위치에서 차량을 살짝 위로 띄우는 값")]
    [SerializeField] private float spawnHeightOffset = 0.5f;

    [Header("소환 공간 검사")]
    [SerializeField]
    private LayerMask vehicleLayer;

    [SerializeField]
    private Vector3 vehicleCheckHalfExtents = new Vector3(1.1f, 0.75f, 2.5f);

    private RoadLaneData[] roads;

    private GameObject currentSummonedVehicle;


    private void Start()
    {
        roads = FindObjectsOfType<RoadLaneData>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(summonKey))
        {
            TrySummonVehicle();
        }
    }


    private void TrySummonVehicle()
    {
        if (currentSummonedVehicle != null)
        {
            VehicleInteractable interactable = currentSummonedVehicle.GetComponentInChildren<VehicleInteractable>(true);

            if (interactable != null && interactable.IsOccupied)
            {
                if (UI_Manager.Instance != null)
                {
                    UI_Manager.Instance.ShowMessage("차량 탑승 중에는 차량을 다시 소환할 수 없습니다.");
                }
                
                return;
            }
        }

        Transform player = GetCurrentPlayer();
        float bestT = 0f;

        if (player == null)
        {
            Debug.LogWarning("[VehicleSummon] 현재 활성 플레이어를 찾지 못했습니다.");

            return;
        }

        if (vehiclePrefab == null)
        {
            Debug.LogWarning("[VehicleSummon] Vehicle Prefab이 연결되지 않았습니다.");

            return;
        }

        bool foundRoad = false;

        float bestDistance = float.MaxValue;

        Vector3 bestPosition = Vector3.zero;
        Vector3 bestDirection = Vector3.forward;

        RoadLaneData bestRoad = null;
        int bestLane = -1;


        foreach (RoadLaneData road in roads)
        {
            if (road == null) continue;

            if (!road.allowVehicleSummon) continue;

            SplineContainer spline = road.GetComponent<SplineContainer>();

            if (spline == null) continue;

            Vector3 localPlayerPosition = spline.transform.InverseTransformPoint(player.position);

            SplineUtility.GetNearestPoint(spline.Spline, localPlayerPosition, out float3 nearestLocalPoint, out float nearestT);

            Vector3 nearestWorldPoint = spline.transform.TransformPoint((Vector3)nearestLocalPoint);

            Vector3 tangent = (Vector3)spline.EvaluateTangent(nearestT);

            tangent.y = 0f;

            if (tangent.sqrMagnitude < 0.001f) continue;

            tangent.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            for (int laneIndex = 0; laneIndex < road.laneCount; laneIndex++)
            {
                float laneOffset = CalculateLaneOffset(laneIndex, road);

                Vector3 lanePosition = nearestWorldPoint + right * laneOffset;

                float distance = Vector3.Distance(player.position, lanePosition);

                if (distance < bestDistance)
                {
                    bestDistance = distance;

                    bestPosition = lanePosition;
                    bestDirection = tangent;

                    bestRoad = road;
                    bestLane = laneIndex;
                    bestT = nearestT;

                    foundRoad = true;
                }
            }
        }

        if (!foundRoad || bestDistance > maxSummonDistance)
        {
            Debug.Log("[VehicleSummon] 근처에 소환 가능한 도로가 없습니다.");

            return;
        }

        if (!TryFindClearSpawnPosition(bestRoad, bestT, bestLane, out Vector3 safePosition, out Vector3 safeDirection))
        {
            Debug.Log("[VehicleSummon] 주변 차선에 차량을 소환할 빈 공간이 없습니다.");

            return;
        }

        SpawnVehicle(safePosition, safeDirection, bestRoad, bestLane, bestDistance);
    }

    private float CalculateLaneOffset(int laneIndex, RoadLaneData road)
    {
        return (laneIndex - (road.laneCount - 1) * 0.5f) * road.laneWidth;
    }

    private bool IsSpawnAreaClear(Vector3 position, Quaternion rotation)
    {
        Vector3 checkCenter = position + Vector3.up * vehicleCheckHalfExtents.y;

        bool blocked = Physics.CheckBox(checkCenter, vehicleCheckHalfExtents, rotation, vehicleLayer, QueryTriggerInteraction.Ignore);

        return !blocked;
    }

    private bool TryFindClearSpawnPosition(RoadLaneData road, float startT, int laneIndex, out Vector3 foundPosition, out Vector3 foundDirection)
    {
        foundPosition = Vector3.zero;
        foundDirection = Vector3.forward;

        SplineContainer spline = road.GetComponent<SplineContainer>();

        if (spline == null) return false;

        float laneOffset = CalculateLaneOffset(laneIndex, road);

        float[] searchDistances = {0f, -5f, 10f, -10f, 15f, -15f};

        for (int i = 0; i < searchDistances.Length; i++)
        {
            float distance = searchDistances[i];

            Vector3 position;
            float candidateT;

            if (Mathf.Approximately(distance, 0f))
            {
                candidateT = startT;

                position = (Vector3)spline.EvaluatePosition(candidateT);
            }
            else
            {
                float3 localPoint = SplineUtility.GetPointAtLinearDistance(spline.Spline, startT, distance, out candidateT);

                position = spline.transform.TransformPoint((Vector3)localPoint);
            }

            Vector3 tangent = (Vector3)spline.EvaluateTangent(candidateT);

            tangent.y = 0f;

            if (tangent.sqrMagnitude < 0.001f) continue;

            tangent.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            position += right * laneOffset;

            Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);

            if (IsSpawnAreaClear(position, rotation))
            {
                foundPosition = position;
                foundDirection = tangent;

                return true;
            }
        }

        return false;
    }

    private void SpawnVehicle(Vector3 position, Vector3 direction, RoadLaneData road, int laneIndex, float distance)
    {
        position += Vector3.up * spawnHeightOffset;

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (currentSummonedVehicle != null)
        {
            Destroy(currentSummonedVehicle);
        }

        currentSummonedVehicle = Instantiate(vehiclePrefab, position, rotation);

        Debug.Log($"[VehicleSummon] 소환 성공 | " + $"Road = {road.name} | " + $"Lane = {laneIndex} | " + $"Distance = {distance:F2}m");
    }

    private Transform GetCurrentPlayer()
    {
        if (BattleManager.instance == null)
        {
            return null;
        }

        GameObject activeCharacter = BattleManager.instance.GetActiveCharacter();

        if (activeCharacter == null)
        {
            return null;
        }

        return activeCharacter.transform;
    }
}