using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class CarLaneController : MonoBehaviour
{
    [Header("차선 오프셋 설정")]
    public float lane1Offset = -1.5f;
    public float lane2Offset = 1.5f;
    public float centerOffset = 0f;
    public float laneChangeDuration = 2f;

    private Transform carMesh;
    private SplineAnimate carAnim;
    private SplineContainer lastContainer;
    private int currentLane;

    // 현재 예약된 차선 변경을 기억해둘 변수
    private Coroutine currentLaneChangeCoroutine;

    private CarSensor carSensor;

    void Start()
    {
        carMesh = transform.Find("Car_Model");
        carAnim = GetComponent<SplineAnimate>();

        carSensor = GetComponent<CarSensor>();

        if (carMesh.localPosition.x < -0.1f) currentLane = 1;
        else if (carMesh.localPosition.x > 0.1f) currentLane = 2;
        else currentLane = 0;

        lastContainer = carAnim.Container;

        // 게임 시작 시 첫 도로에 대한 1회용 차선 변경 타이머 작동
        if (lastContainer != null && lastContainer.gameObject.CompareTag("TwoLaneRoad"))
        {
            currentLaneChangeCoroutine = StartCoroutine(SingleLaneChangeRoutine(lastContainer));
        }
    }

    void Update()
    {
        if (carAnim.Container != lastContainer)
        {
            lastContainer = carAnim.Container;
            HandleContainerChange();
        }
    }

    void HandleContainerChange()
    {
        if (lastContainer == null) return;

        //도로가 바뀌었으므로, 이전 도로의 알람을 강제로 끔.
        if (currentLaneChangeCoroutine != null)
        {
            StopCoroutine(currentLaneChangeCoroutine);
            currentLaneChangeCoroutine = null;
        }

        if (!lastContainer.gameObject.CompareTag("TwoLaneRoad"))
        {
            ChangeLane(0);
        }
        else if (lastContainer.gameObject.CompareTag("TwoLaneRoad"))
        {
            if (currentLane == 0)
            {
                int randomStartLane = Random.Range(1, 3);
                ChangeLane(randomStartLane);
            }

            //새로운 2차선 도로에 진입했으므로, 이 도로 전용알람을 새로 맞춤.
            currentLaneChangeCoroutine = StartCoroutine(SingleLaneChangeRoutine(lastContainer));
        }
    }

    IEnumerator SingleLaneChangeRoutine(SplineContainer currentRoad)
    {
        yield return new WaitForSeconds(Random.Range(3f, 6f));

        if (carAnim.Container == currentRoad && carAnim.Container.gameObject.CompareTag("TwoLaneRoad"))
        {
            if (carSensor != null && (carSensor.isBraking || carSensor.currentSpeed < 0.5f))
            {
                yield break;
            }
            bool shouldChangeLane = Random.value > 0.5f;

            if (shouldChangeLane)
            {
                if (currentLane == 1 || currentLane == 2)
                {
                    int targetLane = (currentLane == 1) ? 2 : 1;
                    if (IsTargetLaneSafe(targetLane))
                    {
                        ChangeLane(targetLane);
                    }
                }
            }
        }
    }

    //옆 차선 확인
    bool IsTargetLaneSafe(int targetLane)
    {
        if (carSensor == null) return true;

        Vector3 checkDirection = (targetLane == 1) ? -transform.right : transform.right;

        Vector3 origin = carMesh.position + Vector3.up * 0.5f;

        float checkDistance = 2.0f;
        float checkRadius = 1.5f;

        LayerMask combinedMask = carSensor.obstacleLayer | carSensor.playerLayer;

        RaycastHit hit;
        if (Physics.SphereCast(origin, checkRadius, checkDirection, out hit, checkDistance, combinedMask))
        {
            if (hit.collider.CompareTag("Car"))
            {
                CarSensor sideCar = hit.collider.GetComponentInParent<CarSensor>();
                if (sideCar != null && sideCar != this.carSensor)
                {
                    return false; 
                }
            }
            else if (hit.collider.CompareTag("Player"))
            {
                return false;
            }
        }
        return true; 
    }

    public void ChangeLane(int targetLane)
    {
        if (currentLane == targetLane) return;

        float targetX = centerOffset;
        if (targetLane == 1) targetX = lane1Offset;
        else if (targetLane == 2) targetX = lane2Offset;

        carMesh.DOLocalMoveX(targetX, laneChangeDuration).SetEase(Ease.InOutSine);
        currentLane = targetLane;
    }
}