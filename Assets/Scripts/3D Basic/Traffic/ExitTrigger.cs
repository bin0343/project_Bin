using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ExitTrigger : MonoBehaviour
{
    [Header("합류할 탈출로 스플라인")]
    public SplineContainer exitRoad;

    [Header("이 Exit로 진입하는 교차로 경로")]
    [SerializeField] private SplineContainer[] sourcePaths;

    [Header("교차로 제어")]
    [SerializeField] private IntersectionController intersectionController;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;

        CarPathFollower pathFollower = other.GetComponentInParent<CarPathFollower>();

        if (pathFollower == null) return;
        if (!IsAllowedSourcePath(pathFollower.CurrentPath)) return;

        pathFollower.QueueNextPath(exitRoad);

        StartCoroutine(WaitForExitPath(pathFollower, exitRoad));
    }

    private bool IsAllowedSourcePath(SplineContainer currentPath)
    {
        if (currentPath == null) return false;

        if (sourcePaths == null || sourcePaths.Length == 0) return true;

        for (int i = 0; i < sourcePaths.Length; i++)
        {
            if (sourcePaths[i] == currentPath) return true;
        }

        return false;
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