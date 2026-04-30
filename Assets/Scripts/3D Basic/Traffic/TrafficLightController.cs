using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TrafficLightController : MonoBehaviour
{
    public enum LightState { Green, Yellow, Red }
    [Header("현재 차량 신호")]
    public LightState currentState = LightState.Green;

    [Header("신호 유지 시간")]
    public float greenDuration = 40f;
    public float yellowDuration = 3f;
    public float redDuration = 10f;

    [Header("보행자 통제용")]
    public GameObject pedestrianBarrierParent;
    private NavMeshObstacle[] pedestrianBarriers;

    private void Start()
    {
        if (pedestrianBarrierParent != null)
        {
            pedestrianBarriers = pedestrianBarrierParent.GetComponentsInChildren<NavMeshObstacle>();
        }
        StartCoroutine(LightCycleRoutine());
    }

    IEnumerator LightCycleRoutine()
    {
        while (true)
        {
            //[차량 초록불] : 차는 지나가고 사람은 멈춤
            currentState = LightState.Green;
            SetPedestrianBarriers(true); // 보행자 길막 켜기
            yield return new WaitForSeconds(greenDuration);

            //[차량 노란불] : 차 정지, 사람 대기
            currentState = LightState.Yellow;
            SetPedestrianBarriers(true);
            yield return new WaitForSeconds(yellowDuration);

            //[차량 빨간불] : 차는 멈추고 사람은 건넘
            currentState = LightState.Red;
            SetPedestrianBarriers(false); // 보행자 길막 끄기
            yield return new WaitForSeconds(redDuration);
        }
    }

    // 횡단보도의 보이지 않는 벽을 켜고 끄는 함수
    void SetPedestrianBarriers(bool isActive)
    {
        foreach (var barrier in pedestrianBarriers)
        {
            if (barrier != null) barrier.enabled = isActive;
        }
    }
}
