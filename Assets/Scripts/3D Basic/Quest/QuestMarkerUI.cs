using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestMarkerUI : MonoBehaviour
{
    public static QuestMarkerUI Instance;

    [Header("UI 컴포넌트")]
    public RectTransform markerRect;
    public Image markerIcon;
    public Text distanceText;

    [Header("추적 대상")]
    public Transform player;    //플레이어와의 거리 계산
    public float heightOffset = 2.0f;   //머리위 높이

    private Transform currentTarget;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            if (markerRect.gameObject.activeSelf) HideMarker();
            return;
        }

        // 1. 타겟의 3D 월드 좌표에 높이 오프셋을 더함
        Vector3 worldPos = currentTarget.position + (Vector3.up * heightOffset);

        // 2. 3D 좌표를 2D 스크린 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 3. 타겟이 카메라 앞쪽(z > 0)에 있을 때만 UI를 표시
        if (screenPos.z > 0)
        {
            if (!markerRect.gameObject.activeSelf) ShowMarker();

            // UI 위치 업데이트
            markerRect.position = screenPos;

            // 플레이어와의 거리 계산 및 텍스트 갱신
            if (player != null && distanceText != null)
            {
                float distance = Vector3.Distance(player.position, currentTarget.position);
                distanceText.text = $"{Mathf.FloorToInt(distance)}m";
            }
        }
        else
        {
            // 타겟이 카메라 뒤로 넘어가면 마커 숨기기
            if (markerRect.gameObject.activeSelf) markerRect.gameObject.SetActive(false);
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
        markerRect.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);

        markerRect.DOAnchorPosY(markerRect.anchoredPosition.y + 15f, 1f)
                  .SetLoops(-1, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine);
    }

    private void HideMarker()
    {
        markerRect.DOKill();
        markerRect.gameObject.SetActive(false);
    }
}
