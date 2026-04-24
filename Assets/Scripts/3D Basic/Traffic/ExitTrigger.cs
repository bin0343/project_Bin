using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ExitTrigger : MonoBehaviour
{
    [Header("합류할 탈출로 스플라인")]
    public SplineContainer exitRoad; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            SplineAnimate carAnim = other.GetComponentInParent<SplineAnimate>();
            if (carAnim == null) return;

            // 탈출로 예약
            StartCoroutine(WaitAndSwap(carAnim, exitRoad));
        }
    }

    IEnumerator WaitAndSwap(SplineAnimate carAnim, SplineContainer nextPath)
    {
        // 교차로 끝까지 대기
        while (carAnim.NormalizedTime < 0.995f)
        {
            yield return null;
        }

        // 곡선 끝에 도달시, 탈출로로 변경
        carAnim.Container = nextPath;
        carAnim.Restart(true);
    }
}