using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class IntersectionTrigger : MonoBehaviour
{
    [Header("1차로(좌측) 차량이 갈 수 있는 경로들")]
    public SplineContainer[] lane1Choices;

    [Header("2차로(우측) 차량이 갈 수 있는 경로들")]
    public SplineContainer[] lane2Choices;

    [Header("교차로 제어")]
    [SerializeField] private IntersectionController intersectionController;

    [Header("교차로 정지 위치")]
    [SerializeField] private Transform stopPoint;

    private readonly HashSet<CarPathFollower> processingVehicles = new HashSet<CarPathFollower>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;

        CarPathFollower pathFollower = other.GetComponentInParent<CarPathFollower>();

        if (pathFollower == null) return;

        Transform carMesh = pathFollower.transform.Find("Car_Model");

        if (carMesh == null) return;

        float offsetX = carMesh.localPosition.x;

        SplineContainer[] targetChoices;

        // 기존 차선 판별 방식 유지
        if (offsetX < -0.1f)
        {
            targetChoices = lane1Choices;
        }
        else if (offsetX > 0.1f)
        {
            targetChoices = lane2Choices;
        }
        else
        {
            targetChoices = lane1Choices.Length > 0 ? lane1Choices : lane2Choices;
        }

        if (targetChoices == null || targetChoices.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, targetChoices.Length);

        SplineContainer chosenPath = targetChoices[randomIndex];

        if (!processingVehicles.Add(pathFollower))return;

        StartCoroutine(RequestIntersectionAndQueuePath(pathFollower, chosenPath));
    }

    private IEnumerator RequestIntersectionAndQueuePath(CarPathFollower follower, SplineContainer chosenPath)
    {
        CarSensor sensor = follower.GetComponent<CarSensor>();

        bool reservationGranted = false;

        while (follower != null && !reservationGranted)
        {
            if (intersectionController == null)
            {
                Debug.LogError($"[{gameObject.name}] " + "IntersectionController가 연결되지 않았습니다.");

                break;
            }

            reservationGranted = intersectionController.RequestEntry(follower);

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

        if (sensor != null)
        {
            sensor.SetIntersectionBlocked(false, Vector3.zero);
        }

        follower.QueueNextPath(chosenPath);

        float timeout = 10f;
        float elapsed = 0f;

        while (follower != null && follower.CurrentPath != chosenPath)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= timeout)
            {
                processingVehicles.Remove(follower);
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