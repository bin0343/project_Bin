using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestMarkerUI : MonoBehaviour
{
    public static QuestMarkerUI instance;

    public Transform CurrentTarget => currentTarget;

    [Header("UI 컴포넌트 연결")]
    public RectTransform markerRect;
    public Image markerIcon;
    public Text distanceText;

    [Header("추적 대상 설정")]
    public Transform player;
    public float heightOffset = 2.0f;
    public float screenPadding = 80f; // 화면 테두리 여백 (테두리에서 얼마나 떨어질지)

    [Header("원근감(3D 일체감) 설정")]
    public float maxScaleDistance = 5f;
    public float minScaleDistance = 50f;
    public float maxScale = 1.0f;
    public float minScale = 0.5f;

    [Header("추적 종료 설정")]
    public float arriveDistance = 1.0f; // 도착으로 인정할 거리 (1m)
    private Transform playerTransform;

    private Transform currentTarget;
    private Camera mainCam;
    private Sprite defaultIcon;

    private void Awake()
    {
        if (instance == null) instance = this;
        mainCam = Camera.main;
        if (markerIcon != null) defaultIcon = markerIcon.sprite;
        HideMarker();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            if (markerRect.gameObject.activeSelf) HideMarker();
            return;
        }

        if (playerTransform != null)
        {
            Vector3 playerPos = playerTransform.position;
            Vector3 targetPos = currentTarget.position;

            playerPos.y = 0f;
            targetPos.y = 0f;

            float distance = Vector3.Distance(playerPos, targetPos);

            if (distance <= arriveDistance)
            {
                ClearTarget(); // 추적 종료 함수 호출
                return;        // 이번 프레임은 여기서 중단
            }
        }

        // 1. 3D 월드 좌표 계산 (상하 둥둥 효과 포함)
        Vector3 worldPos = currentTarget.position + (Vector3.up * heightOffset);

        // 2. 월드 좌표 -> 스크린 2D 좌표 변환
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        // 3. 화면 밖으로 나갔는지(오프스크린) 판별
        bool isOffScreen = screenPos.z < 0 ||
                           screenPos.x < 0 || screenPos.x > Screen.width ||
                           screenPos.y < 0 || screenPos.y > Screen.height;

        if (!markerRect.gameObject.activeSelf) ShowMarker();

        float distanceToCamera = Vector3.Distance(mainCam.transform.position, worldPos);
        float currentScale = maxScale;

        if (isOffScreen)
        {
            // 오프스크린 인디케이터 로직

            // 타겟이 카메라 뒤통수에 있으면 방향을 반전시킵니다.
            if (screenPos.z < 0)
            {
                screenPos *= -1;
            }

            // 화면 중심점을 기준으로 방향 벡터 계산
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Vector3 dir = screenPos - screenCenter;
            dir.z = 0; // Z축 무시

            // 벡터가 0이 되는 예외 상황 방지
            if (dir == Vector3.zero) dir = Vector3.up;

            // 기울기(가로/세로 비율)를 이용해 테두리 교차점 계산
            float slope = dir.y / dir.x;

            // 패딩(여백)을 적용한 실제 한계치
            float minX = screenPadding;
            float maxX = Screen.width - screenPadding;
            float minY = screenPadding;
            float maxY = Screen.height - screenPadding;

            Vector3 clampedPos = screenCenter;

            // X축 경계 기준 교차점 찾기
            if (dir.x > 0)
            {
                clampedPos.x = maxX;
                clampedPos.y = screenCenter.y + (maxX - screenCenter.x) * slope;
            }
            else
            {
                clampedPos.x = minX;
                clampedPos.y = screenCenter.y + (minX - screenCenter.x) * slope;
            }

            // Y축 경계를 벗어난다면 Y축 기준으로 다시 교차점 잡기
            if (clampedPos.y > maxY)
            {
                clampedPos.y = maxY;
                clampedPos.x = screenCenter.x + (maxY - screenCenter.y) / slope;
            }
            else if (clampedPos.y < minY)
            {
                clampedPos.y = minY;
                clampedPos.x = screenCenter.x + (minY - screenCenter.y) / slope;
            }

            screenPos = clampedPos;

            // 화면 가장자리에 붙을 때는 UI가 거슬리지 않도록 최소 크기로 고정합니다.
            currentScale = minScale;
        }
        else
        {
            // 온스크린(시야 안) 로직: 원근감 적용
            float scaleRatio = Mathf.InverseLerp(minScaleDistance, maxScaleDistance, distanceToCamera);
            currentScale = Mathf.Lerp(minScale, maxScale, scaleRatio);
        }

        // 최종 위치 및 크기 적용
        markerRect.position = screenPos;
        markerRect.localScale = new Vector3(currentScale, currentScale, 1f);

        // 플레이어와의 거리 텍스트 업데이트
        if (player != null && distanceText != null)
        {
            float distanceToPlayer = Vector3.Distance(player.position, currentTarget.position);
            distanceText.text = $"{Mathf.FloorToInt(distanceToPlayer)}m";
        }
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
        ShowMarker();
    }

    public void ClearTarget()
    {
        currentTarget = null;
        HideMarker();
    }

    private void ShowMarker()
    {
        markerRect.gameObject.SetActive(true);
        markerRect.localScale = Vector3.zero;
        markerRect.DOScale(maxScale, 0.4f).SetEase(Ease.OutBack);
    }

    private void HideMarker()
    {
        markerRect.DOKill();
        markerRect.gameObject.SetActive(false);
    }

    public void SetTarget(Transform newTarget, Sprite overrideIcon = null)
    {
        currentTarget = newTarget;

        if (markerIcon != null)
        {
            markerIcon.sprite = (overrideIcon != null) ? overrideIcon : defaultIcon;
        }

        ShowMarker();
    }
}