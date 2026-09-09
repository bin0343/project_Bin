using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionTrigger : MonoBehaviour
{
    [Header("진입 방향")]
    [SerializeField, Min(0)] private int approachId;

    [Header("1차로 차량이 선택 가능한 경로")]
    public IntersectionPathChoice[] lane1Choices;

    [Header("2차로 차량이 선택 가능한 경로")]
    public IntersectionPathChoice[] lane2Choices;

    [Header("교차로 제어")]
    [SerializeField] private IntersectionController intersectionController;

    [Header("교차로 정지 위치")]
    [SerializeField] private Transform stopPoint;

    [Header("신호 제어")]
    [SerializeField] private TrafficLightController trafficLight;

    private readonly HashSet<CarPathFollower> processingVehicles = new HashSet<CarPathFollower>();
    


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;

        CarPathFollower pathFollower = other.GetComponentInParent<CarPathFollower>();

        if (pathFollower == null) return;

        CarLaneController laneController = pathFollower.GetComponent<CarLaneController>();

        int laneId = laneController != null ? laneController.CurrentLane : 0;

        IntersectionPathChoice[] targetChoices;

        if (laneId == 0)
        {
            targetChoices = lane1Choices;
        }
        else if (laneId == 1)
        {
            targetChoices = lane2Choices;
        }
        else
        {
            return;
        }

        int randomIndex = Random.Range(0, targetChoices.Length);

        IntersectionPathChoice chosenChoice = targetChoices[randomIndex];

        if (chosenChoice == null || chosenChoice.path == null)
        {
            return;
        }

        if (!processingVehicles.Add(pathFollower)) return;

        StartCoroutine(RequestIntersectionAndQueuePath(pathFollower, laneId, chosenChoice));
    }


    private IEnumerator RequestIntersectionAndQueuePath(CarPathFollower follower, int laneId, IntersectionPathChoice chosenChoice)
    {
        CarSensor sensor = follower.GetComponent<CarSensor>();

        bool reservationGranted = false;

        while (follower != null && !reservationGranted)
        {
            if (intersectionController == null)
            {
                Debug.LogError($"[{gameObject.name}] " + "IntersectionController가 연결되지 않았습니다.");

                processingVehicles.Remove(follower);
                yield break;
            }

            bool canProceedSignal = sensor == null || trafficLight == null || sensor.CanProceedThroughSignal(trafficLight, stopPoint.position);

            if (!canProceedSignal)
            {
                if (sensor != null && stopPoint != null)
                {
                    sensor.SetIntersectionBlocked(true, stopPoint.position);
                }

                yield return null;
                continue;
            }

            reservationGranted = intersectionController.RequestEntry(follower, approachId, laneId, chosenChoice.movementId, chosenChoice.turnType);

            if (!reservationGranted)
            {
                if (sensor != null && stopPoint != null)
                {
                    sensor.SetIntersectionBlocked(true, stopPoint.position);
                }

                yield return null;
            }
        }

        if (follower == null) yield break;

        // 교차로 진입 허가
        if (sensor != null)
        {
            sensor.SetIntersectionCommitment(true);

            sensor.SetIntersectionBlocked(false, Vector3.zero);
        }

        follower.QueueNextPath(chosenChoice.path);

        float timeout = 10f;
        float elapsed = 0f;

        while (follower != null && follower.CurrentPath != chosenChoice.path)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= timeout)
            {
                // 이미 받은 교차로 예약권 반환
                if (intersectionController != null)
                {
                    intersectionController.Release(follower);
                }

                // 신호 무시 상태도 원상복구
                if (sensor != null)
                {
                    sensor.SetIntersectionCommitment(false);
                    sensor.SetIntersectionBlocked(false, Vector3.zero);
                }

                // 이 차량의 Trigger 처리도 종료
                processingVehicles.Remove(follower);

                Debug.LogWarning($"[{gameObject.name}] {follower.name}의 교차로 경로 전환이 " + $"{timeout}초 안에 완료되지 않아 예약을 취소했습니다.");

                yield break;
            }

            yield return null;
        }

        if (follower == null) yield break;

        if (sensor != null)
        {
            sensor.isInsideIntersection = true;
        }

        processingVehicles.Remove(follower);
    }
}