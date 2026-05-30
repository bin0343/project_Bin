using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapNPCManager : MonoBehaviour
{
    [Header("미니맵 마커 프리팹")]
    [Tooltip("미니맵 Content 자식으로 생성될 NPC 마커 프리팹 (전체맵보다 약간 작게 세팅 추천)")]
    public GameObject miniMapNpcMarkerPrefab;

    [Header("마커 아이콘 스프라이트")]
    public Sprite iconAvailable; // 퀘스트 수락 가능 (!)
    public Sprite iconComplete;  // 퀘스트 완료 보상 가능 (?)

    [Header("미니맵 Content 연결")]
    public RectTransform miniMapContent;

    private List<GameObject> activeMarkers = new List<GameObject>();

    private void Start()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestAccepted += HandleQuestEvent;
            QuestManager.instance.OnQuestCompleted += HandleQuestEventWithStatus;
            QuestManager.instance.OnQuestRewardClaimed += HandleQuestEvent;
        }

        RefreshMiniMapMarkers();
    }

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestAccepted -= HandleQuestEvent;
            QuestManager.instance.OnQuestCompleted -= HandleQuestEventWithStatus;
            QuestManager.instance.OnQuestRewardClaimed -= HandleQuestEvent;
        }
    }

    private void HandleQuestEvent(Quest quest) => RefreshMiniMapMarkers();
    private void HandleQuestEventWithStatus(PlayerQuestStatus status, Quest quest) => RefreshMiniMapMarkers();

    public void RefreshMiniMapMarkers()
    {
        if (LocalMapController.instance == null)
        {
            LocalMapController.instance = FindObjectOfType<LocalMapController>(true);
        }
        // 기존 미니맵 마커 청소
        foreach (GameObject marker in activeMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        activeMarkers.Clear();

        if (LocalMapController.instance == null || miniMapContent == null || miniMapNpcMarkerPrefab == null) return;

        // 씬의 모든 NPC_QuestGiver 탐색
        NPC_QuestGiver[] allNPCs = FindObjectsOfType<NPC_QuestGiver>();

        foreach (NPC_QuestGiver npc in allNPCs)
        {
            if (npc.questToGive == null) continue;

            QuestStatus status = QuestManager.instance.GetQuestStatus(npc.questToGive.questID);
            Sprite targetSprite = null;

            if (status == QuestStatus.NOT_STARTED)
            {
                targetSprite = iconAvailable;
            }
            else if (status == QuestStatus.COMPLETED)
            {
                targetSprite = iconComplete;
            }

            if (targetSprite != null)
            {
                GameObject newMarker = Instantiate(miniMapNpcMarkerPrefab, miniMapContent);
                activeMarkers.Add(newMarker);

                Image markerImage = newMarker.GetComponent<Image>();
                if (markerImage != null) markerImage.sprite = targetSprite;

                RectTransform markerRect = newMarker.GetComponent<RectTransform>();
                if (markerRect != null)
                {
                    markerRect.anchorMin = new Vector2(0.5f, 0.5f);
                    markerRect.anchorMax = new Vector2(0.5f, 0.5f);
                    markerRect.pivot = new Vector2(0.5f, 0.5f);

                    markerRect.localScale = Vector3.one;

                    Vector2 mapPos = LocalMapController.instance.GetMapPosition(npc.transform.position);
                    markerRect.anchoredPosition = mapPos;
                }

                if (markerImage != null) markerImage.raycastTarget = false;
            }
        }
    }
}