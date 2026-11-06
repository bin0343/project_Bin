using UnityEngine;

public class LocalMapMarker : MonoBehaviour
{
    public static LocalMapMarker instance;

    [Header("전체 맵 전용 마커 UI")]
    public RectTransform markerRect; // 전체 맵 위에 띄울 고양이 아이콘 UI

    private string currentTargetID = "";
    private LocalMapController mapController;
    private Transform directTarget;

    private void Awake()
    {
        if (instance == null) instance = this;
        mapController = GetComponent<LocalMapController>();
        HideMarker();
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(currentTargetID))
        {
            UpdateMarkerPosition();
        }
    }

    private void OnEnable()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.RefreshTrackedQuestTarget();
        }
        else
        {
            UpdateMarkerPosition();
        }
    }

    public void SetTargetID(string targetID)
    {
        directTarget = null;

        currentTargetID = targetID;
        UpdateMarkerPosition();
    }

    public void SetTarget(Transform target)
    {
        directTarget = target;

        currentTargetID = "";

        UpdateMarkerPosition();
    }

    public void ClearTarget()
    {
        directTarget = null;
        currentTargetID = "";

        HideMarker();
    }

    public void UpdateMarkerPosition()
    {
        if (markerRect == null || mapController == null)
        {
            HideMarker();
            return;
        }

        Transform targetTr = directTarget;

        if (targetTr == null && !string.IsNullOrEmpty(currentTargetID))
        {
            targetTr = QuestTargetMarker.GetTarget(currentTargetID);
        }

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