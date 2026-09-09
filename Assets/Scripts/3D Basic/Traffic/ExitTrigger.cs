using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ExitTrigger : MonoBehaviour
{
    [Header("합류할 탈출로 스플라인")]
    public SplineContainer exitRoad;

    [Header("교차로 제어")]
    [SerializeField] private IntersectionController intersectionController;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;

        CarPathFollower pathFollower = other.GetComponentInParent<CarPathFollower>();

        if (pathFollower == null) return;

        pathFollower.QueueNextPath(exitRoad);

        StartCoroutine(WaitForExitPath(pathFollower, exitRoad));
    }

    private IEnumerator WaitForExitPath(CarPathFollower follower, SplineContainer roadPath)
    {
        float timeout = 10f;
        float elapsed = 0f;

        while (follower != null && follower.CurrentPath != roadPath)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= timeout) yield break;

            yield return null;
        }

        if (follower == null)
            yield break;

        CarSensor sensor = follower.GetComponent<CarSensor>();

        if (sensor != null)
        {
            sensor.isInsideIntersection = false;

            sensor.SetIntersectionCommitment(false);
        }

        if (intersectionController != null)
        {
            intersectionController.Release(follower);
        }
    }
}