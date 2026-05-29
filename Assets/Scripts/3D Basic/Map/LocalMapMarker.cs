using UnityEngine;

public class LocalMapMarker : MonoBehaviour
{
    public static LocalMapMarker instance;

    [Header("전체 맵 전용 마커 UI")]
    public RectTransform markerRect; // 전체 맵 위에 띄울 고양이 아이콘 UI

    private string currentTargetID = "";
    private LocalMapController mapController;

    private void Awake()
    {
        if (instance == null) instance = this;
        mapController = GetComponent<LocalMapController>();
        HideMarker();
    }

    private void OnEnable()
    {
        // M키를 눌러 맵 패널이 켜질 때 마커 위치 즉시 갱신
        UpdateMarkerPosition();
    }

    public void SetTargetID(string targetID)
    {
        currentTargetID = targetID;
        UpdateMarkerPosition();
    }

    public void ClearTarget()
    {
        currentTargetID = "";
        HideMarker();
    }

    public void UpdateMarkerPosition()
    {
        if (string.IsNullOrEmpty(currentTargetID) || markerRect == null || mapController == null)
        {
            HideMarker();
            return;
        }

        Transform targetTr = QuestTargetMarker.GetTarget(currentTargetID);

        if (targetTr != null && targetTr.gameObject.activeInHierarchy)
        {
            Vector2 mapPos = mapController.GetMapPosition(targetTr.position);
            markerRect.gameObject.SetActive(true);
            markerRect.anchoredPosition = mapPos;
        }
        else
        {
            HideMarker();
        }
    }

    private void HideMarker()
    {
        if (markerRect != null) markerRect.gameObject.SetActive(false);
    }
}