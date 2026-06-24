using UnityEngine;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour
{
    [Header("UI 연결")]
    public RectTransform miniMapContent; // 맵 도화지
    public RectTransform playerIcon;     // 중앙 고정 화살표

    [Header("미니맵 줌 설정")]
    public float minimapZoom = 1.5f;     // 미니맵 전용 확대 배율

    private Transform playerPositionTarget;
    private Transform playerRotationTarget;

    private void Start()
    {
        if (LocalMapController.instance == null)
        {
            LocalMapController.instance = FindObjectOfType<LocalMapController>(true);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerPositionTarget = playerObj.transform;
            Transform realModel = playerObj.transform.Find("Player");
            playerRotationTarget = (realModel != null) ? realModel : playerObj.transform;
        }
    }

    private void LateUpdate()
    {
        // LocalMapController가 아직 없거나 타겟이 없으면 중단
        if (playerPositionTarget == null || LocalMapController.instance == null) return;

        Vector2 playerMapPos = LocalMapController.instance.GetMapPosition(playerPositionTarget.position);

        // 핵심: 플레이어 아이콘을 움직이는 게 아니라, 맵 도화지를 반대로 밀어버립니다!
        if (miniMapContent != null)
        {
            // 확대 배율(Zoom)을 곱해서 반대 방향(-)으로 이동
            miniMapContent.anchoredPosition = -playerMapPos * minimapZoom;

            // 미니맵 도화지 크기도 배율에 맞게 조정 (항상 일정하게 확대된 상태 유지)
            miniMapContent.localScale = new Vector3(minimapZoom, minimapZoom, 1f);
        }

        // 플레이어 화살표 회전 동기화 (머리가 바라보는 방향)
        if (playerIcon != null && playerRotationTarget != null)
        {
            float playerRotationY = playerRotationTarget.eulerAngles.y;
            playerIcon.localEulerAngles = new Vector3(0f, 0f, -playerRotationY);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        playerPositionTarget = newTarget;
        Transform realModel = newTarget.Find("Player");
        playerRotationTarget = (realModel != null) ? realModel : newTarget;
    }
}