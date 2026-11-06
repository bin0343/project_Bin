using UnityEngine;

public class MiniMapQuestMarker : MonoBehaviour
{
    public static MiniMapQuestMarker instance;

    [Header("¹Ì´Ï¸Ê Äù½ºÆ® ¸ñÇ¥ ¸¶Ä¿")]
    public RectTransform markerRect;

    private string currentTargetID = "";
    private Transform directTarget;

    private void Awake()
    {
        if (instance == null) instance = this;

        HideMarker();
    }

    private void OnEnable()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.RefreshTrackedQuestTarget();
        }
    }

    private void LateUpdate()
    {
        if (!string.IsNullOrEmpty(currentTargetID))
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

    private void UpdateMarkerPosition()
    {
        if (markerRect == null || LocalMapController.instance == null)
        {
            HideMarker();
            return;
        }

        /*Transform target = QuestTargetMarker.GetTarget(currentTargetID);

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            HideMarker();
            return;
        }*/

        Transform target = directTarget;

        if (target == null && !string.IsNullOrEmpty(currentTargetID))
        {
            target = QuestTargetMarker.GetTarget(currentTargetID);
        }

        Vector2 mapPos = LocalMapController.instance.GetMapPosition(target.position);

        markerRect.gameObject.SetActive(true);
        markerRect.anchoredPosition = mapPos;
    }

    private void HideMarker()
    {
        if (markerRect != null) markerRect.gameObject.SetActive(false);
    }
}