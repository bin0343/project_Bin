using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class IntersectionTrigger : MonoBehaviour
{
    [Header("1차로(좌측) 차량이 갈 수 있는 경로들")]
    public SplineContainer[] lane1Choices;

    [Header("2차로(우측) 차량이 갈 수 있는 경로들")]
    public SplineContainer[] lane2Choices;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            SplineAnimate carAnim = other.GetComponentInParent<SplineAnimate>();
            if (carAnim == null) return;

            Transform carMesh = other.transform.Find("Car_Model");
            if (carMesh == null) return;

            float offsetX = carMesh.localPosition.x;
            SplineContainer[] targetChoices = null;

            // 차선 판별
            if (offsetX < -0.1f) targetChoices = lane1Choices;
            else if (offsetX > 0.1f) targetChoices = lane2Choices;
            else targetChoices = (lane1Choices.Length > 0) ? lane1Choices : lane2Choices;

            if (targetChoices == null || targetChoices.Length == 0)
                targetChoices = (lane1Choices.Length > 0) ? lane1Choices : lane2Choices;

            int rand = Random.Range(0, targetChoices.Length);
            SplineContainer chosenPath = targetChoices[rand];

            StartCoroutine(WaitAndSwap(carAnim, chosenPath));
        }
    }

    IEnumerator WaitAndSwap(SplineAnimate carAnim, SplineContainer nextPath)
    {
        float lastTime = carAnim.NormalizedTime;

        while (carAnim.NormalizedTime < 0.995f)
        {
            if (carAnim.NormalizedTime < lastTime)
            {
                break;
            }

            lastTime = carAnim.NormalizedTime;
            yield return null;
        }

        // 끝에 도달 시 탈출로로 변경
        carAnim.Container = nextPath;
        carAnim.Restart(true);
    }
}