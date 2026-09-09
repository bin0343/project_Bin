using System.Collections.Generic;
using UnityEngine;

public class IntersectionController : MonoBehaviour
{
    [System.Serializable]
    public struct CompatibleMovementPair
    {
        [Min(0)]
        public int movementA;

        [Min(0)]
        public int movementB;
    }

    private class IntersectionRequest
    {
        public CarPathFollower vehicle;

        public int approachId;
        public int laneId;
        public int movementId;

        public IntersectionTurnType turnType;

        public float requestedAt;
        public ulong requestOrder;
    }

    [Header("요청 수집")]
    [Tooltip("거의 동시에 도착한 차량들을 잠깐 모아서 " + "우선순위를 비교하는 시간")]
    [SerializeField] private float requestCollectionTime = 0.15f;


    [Header("우선순위 - 낮을수록 우선")]
    [SerializeField] private int straightPriority = 0;
    [SerializeField] private int leftPriority = 1;
    [SerializeField] private int rightPriority = 2;

    [Header("연속 진입")]
    [Tooltip("같은 Movement 차량이 교차로에 연속 진입할 최소 시간 간격")]
    [SerializeField] private float sameMovementHeadway = 0.8f;

    [Tooltip("같은 차선 차량들의 최소 진입 시간 간격")]
    [SerializeField] private float sameLaneHeadway = 0.7f;

    [Header("Starvation 방지")]
    [Tooltip("이 시간 이상 기다린 차량은 " + "회전 우선순위를 무시하고 가장 오래 기다린 요청부터 처리")]
    [SerializeField] private float starvationOverrideTime = 8f;


    [Header("동시 통과 가능한 Movement")]
    [SerializeField] private CompatibleMovementPair[] compatiblePairs;

    private readonly List<IntersectionRequest> waitingRequests = new List<IntersectionRequest>();
    private readonly Dictionary<CarPathFollower, IntersectionRequest> waitingByVehicle = new Dictionary<CarPathFollower, IntersectionRequest>();
    private readonly Dictionary<CarPathFollower, IntersectionRequest> activeReservations = new Dictionary<CarPathFollower, IntersectionRequest>();
    private readonly Dictionary<int, float> lastGrantTimeByMovement = new Dictionary<int, float>();
    private readonly Dictionary<Vector2Int, float> lastGrantTimeByLane = new Dictionary<Vector2Int, float>();
    private readonly List<IntersectionRequest> batchBuffer = new List<IntersectionRequest>();
    private readonly List<CarPathFollower> invalidVehicleBuffer = new List<CarPathFollower>();
    private readonly List<IntersectionRequest> candidateBuffer = new List<IntersectionRequest>();

    private float nextDecisionTime = -1f;
    private ulong requestOrderCounter = 0;

    public int WaitingCount => waitingRequests.Count;

    public int ActiveCount => activeReservations.Count;

    public bool IsOccupied => activeReservations.Count > 0;

    private void Update()
    {
        CleanInvalidVehicles();

        //교차로 사용 중
        if (activeReservations.Count > 0)
        {
            TryExtendActiveBatch();
            return;
        }

        if (waitingRequests.Count == 0)
        {
            nextDecisionTime = -1f;
            return;
        }

        // 처음 대기 차량이 들어온 뒤
        // 잠깐 다른 요청들도 모은다.
        if (nextDecisionTime < 0f)
        {
            nextDecisionTime = Time.time + requestCollectionTime;

            return;
        }

        if (Time.time >= nextDecisionTime)
        {
            GrantNextBatch();
        }
    }


    #region Request

    public bool RequestEntry(CarPathFollower vehicle, int approachId, int laneId, int movementId, IntersectionTurnType turnType)
    {
        if (!IsValidVehicle(vehicle)) return false;

        CleanInvalidVehicles();

        // 이미 이번 Batch에서 승인됨
        if (activeReservations.ContainsKey(vehicle))
        {
            return true;
        }

        // 이미 대기 중
        if (waitingByVehicle.ContainsKey(vehicle))
        {
            return false;
        }

        IntersectionRequest request = new IntersectionRequest
        {
            vehicle = vehicle,

            approachId = approachId,
            laneId = laneId,
            movementId = movementId,

            turnType = turnType,

            requestedAt = Time.time,
            requestOrder = requestOrderCounter++
        };

        waitingRequests.Add(request);

        waitingByVehicle.Add(vehicle, request);

        if (activeReservations.Count == 0 && nextDecisionTime < 0f)
        {
            nextDecisionTime = Time.time + requestCollectionTime;
        }

        return false;
    }

    public void Release(CarPathFollower vehicle)
    {
        if (vehicle == null) return;

        activeReservations.Remove(vehicle);

        // 이번 Batch 차량이 전부 빠져나감
        if (activeReservations.Count == 0 && waitingRequests.Count > 0)
        {
            nextDecisionTime = Time.time + requestCollectionTime;
        }
    }

    private bool HasBlockingWaitingRequest()
    {
        for (int i = 0; i < waitingRequests.Count; i++)
        {
            IntersectionRequest waiting = waitingRequests[i];

            if (!IsLaneHead(waiting)) continue;

            if (!IsMovementCompatibleWithActive(waiting)) return true;
        }

        return false;
    }

    private bool IsMovementCompatibleWithActive(IntersectionRequest candidate)
    {
        foreach (var pair in activeReservations)
        {
            IntersectionRequest active = pair.Value;

            // 같은 진입로 + 같은 차선의 앞차라면
            // Movement가 달라도 추종 관계로 취급한다.
            if (candidate.approachId == active.approachId && candidate.laneId == active.laneId) continue;

            // 다른 차량 흐름과는 실제 Conflict 관계 검사
            if (!AreMovementsCompatible(candidate.movementId, active.movementId)) return false;
        }

        return true;
    }

    private bool HasEnoughHeadway(int movementId)
    {
        if (!lastGrantTimeByMovement.TryGetValue(movementId, out float lastGrantTime))
        {
            return true;
        }

        return Time.time - lastGrantTime >= sameMovementHeadway;
    }

    private bool HasEnoughLaneHeadway(IntersectionRequest request)
    {
        Vector2Int laneKey = new Vector2Int(request.approachId, request.laneId);

        if (!lastGrantTimeByLane.TryGetValue(laneKey, out float lastGrantTime)) return true;

        return Time.time - lastGrantTime >= sameLaneHeadway;
    }

    #endregion


    #region Batch
    private void GrantNextBatch()
    {
        if (activeReservations.Count > 0 || waitingRequests.Count == 0) return;

        IntersectionRequest seed = SelectSeedRequest();

        if (seed == null) return;

        batchBuffer.Clear();
        batchBuffer.Add(seed);

        candidateBuffer.Clear();

        for (int i = 0; i < waitingRequests.Count; i++)
        {
            IntersectionRequest candidate = waitingRequests[i];

            if (candidate == seed) continue;

            if (!IsLaneHead(candidate)) continue;

            candidateBuffer.Add(candidate);
        }

        candidateBuffer.Sort((a, b) =>
        {
            int priorityCompare = GetPriority(a.turnType).CompareTo(GetPriority(b.turnType));

            if (priorityCompare != 0)
                return priorityCompare;

            return a.requestOrder.CompareTo(b.requestOrder);
        });

        for (int i = 0; i < candidateBuffer.Count; i++)
        {
            IntersectionRequest candidate = candidateBuffer[i];

            if (CanJoinBatch(candidate))
            {
                batchBuffer.Add(candidate);
            }
        }

        // 선정된 Batch 활성화
        for (int i = 0; i < batchBuffer.Count; i++)
        {
            GrantRequest(batchBuffer[i]);
        }

        nextDecisionTime = -1f;
    }

    private void GrantRequest(IntersectionRequest request)
    {
        if (request == null) return;

        waitingRequests.Remove(request);

        waitingByVehicle.Remove(request.vehicle);

        activeReservations[request.vehicle] = request;

        lastGrantTimeByMovement[request.movementId] = Time.time;

        Vector2Int laneKey = new Vector2Int(request.approachId, request.laneId);

        lastGrantTimeByLane[laneKey] = Time.time;
    }


    private bool CanJoinBatch(IntersectionRequest candidate)
    {
        for (int i = 0; i < batchBuffer.Count; i++)
        {
            IntersectionRequest activeCandidate = batchBuffer[i];

            if (!AreMovementsCompatible(candidate.movementId, activeCandidate.movementId))
            {
                return false;
            }
        }

        return true;
    }

    private void TryExtendActiveBatch()
    {
        if (waitingRequests.Count == 0) return;

        if (HasBlockingWaitingRequest()) return;


        for (int i = 0; i < waitingRequests.Count; i++)
        {
            IntersectionRequest candidate = waitingRequests[i];

            if (!IsLaneHead(candidate)) continue;

            if (!IsMovementCompatibleWithActive(candidate)) continue;

            if (!HasEnoughHeadway(candidate.movementId)) continue;

            if (!HasEnoughLaneHeadway(candidate)) continue;

            GrantRequest(candidate);

            i--;
        }
    }

    #endregion


    #region Priority

    private IntersectionRequest SelectSeedRequest()
    {
        IntersectionRequest starvationRequest = FindStarvationRequest();

        // 오래 기다린 차량이 있다면 우선
        if (starvationRequest != null)
        {
            return starvationRequest;
        }

        IntersectionRequest best = null;
        int bestPriority = int.MaxValue;

        for (int i = 0; i < waitingRequests.Count; i++)
        {
            IntersectionRequest request = waitingRequests[i];

            if (!IsLaneHead(request)) continue;

            int priority = GetPriority(request.turnType);

            if (best == null || priority < bestPriority)
            {
                best = request;
                bestPriority = priority;

                continue;
            }

            // 같은 우선순위면 먼저 도착한 차량
            if (priority == bestPriority && request.requestOrder < best.requestOrder)
            {
                best = request;
            }
        }

        return best;
    }


    private IntersectionRequest FindStarvationRequest()
    {
        IntersectionRequest oldest = null;

        for (int i = 0; i < waitingRequests.Count; i++)
        {
            IntersectionRequest request = waitingRequests[i];

            if (!IsLaneHead(request)) continue;

            float waitTime = Time.time - request.requestedAt;

            if (waitTime < starvationOverrideTime) continue;

            if (oldest == null || request.requestOrder < oldest.requestOrder)
            {
                oldest = request;
            }
        }

        return oldest;
    }


    private int GetPriority(IntersectionTurnType turnType)
    {
        switch (turnType)
        {
            case IntersectionTurnType.Straight:
                return straightPriority;

            case IntersectionTurnType.Left:
                return leftPriority;

            case IntersectionTurnType.Right:
                return rightPriority;
        }

        return int.MaxValue;
    }

    private bool IsLaneHead(IntersectionRequest request)
    {
        for (int i = 0; i < waitingRequests.Count; i++)
        {
            IntersectionRequest other = waitingRequests[i];

            if (other == request) continue;

            // 다른 진입로
            if (other.approachId != request.approachId) continue;

            // 다른 차선
            if (other.laneId != request.laneId) continue;

            // 같은 진입로 + 같은 차선에
            // 나보다 먼저 요청한 차가 있으면
            // 나는 아직 Head가 아님
            if (other.requestOrder < request.requestOrder) return false;
        }

        return true;
    }

    #endregion


    #region Conflict

    private bool AreMovementsCompatible(int movementA, int movementB)
    {
        if (movementA == movementB) return false;

        for (int i = 0; i < compatiblePairs.Length; i++)
        {
            CompatibleMovementPair pair = compatiblePairs[i];

            bool forward = pair.movementA == movementA && pair.movementB == movementB;

            bool reverse = pair.movementA == movementB && pair.movementB == movementA;

            if (forward || reverse)
            {
                return true;
            }
        }
        return false;
    }

    #endregion


    #region Cleanup

    private void CleanInvalidVehicles()
    {
        invalidVehicleBuffer.Clear();

        foreach (var pair in activeReservations)
        {
            if (!IsValidVehicle(pair.Key))
            {
                invalidVehicleBuffer.Add(pair.Key);
            }
        }

        for (int i = 0; i < invalidVehicleBuffer.Count; i++)
        {
            activeReservations.Remove(invalidVehicleBuffer[i]);
        }


        for (int i = waitingRequests.Count - 1; i >= 0; i--)
        {
            IntersectionRequest request = waitingRequests[i];

            if (IsValidVehicle(request.vehicle)) continue;

            waitingByVehicle.Remove(request.vehicle);

            waitingRequests.RemoveAt(i);
        }

        if (activeReservations.Count == 0 && waitingRequests.Count > 0 && nextDecisionTime < 0f)
        {
            nextDecisionTime = Time.time + requestCollectionTime;
        }
    }


    private bool IsValidVehicle(CarPathFollower vehicle)
    {
        return vehicle != null && vehicle.gameObject.activeInHierarchy;
    }

    #endregion
}