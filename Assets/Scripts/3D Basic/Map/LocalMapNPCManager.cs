using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalMapNPCManager : MonoBehaviour
{
    [Header("맵 마커 프리팹")]
    [Tooltip("전체 맵 Content 자식으로 생성될 NPC 마커 프리팹")]
    public GameObject npcMarkerPrefab;

    [Header("마커 아이콘 스프라이트")]
    public Sprite iconAvailable; // 퀘스트 수락 가능 (!)
    public Sprite iconComplete;  // 퀘스트 완료 보상 가능 (?)

    private LocalMapController mapController;
    private RectTransform mapContent;

    // 현재 지도 위에 생성되어 있는 마커들을 관리하는 리스트
    private List<GameObject> activeMarkers = new List<GameObject>();

    private void Awake()
    {
        mapController = GetComponent<LocalMapController>();

        // LocalMapController에 있는 mapContent(도화지) 가져오기
        // serialized 필드 접근을 위해 인스펙터 구조를 고려해 가져옵니다.
        var fields = typeof(LocalMapController).GetField("mapContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fields != null) mapContent = (RectTransform)fields.GetValue(mapController);
    }

    private void Start()
    {
        if (QuestManager.instance == null) return;

        QuestManager.instance.OnQuestAccepted += HandleQuestEvent;
        QuestManager.instance.OnQuestCompleted += HandleQuestEventWithStatus;
        QuestManager.instance.OnQuestRewardClaimed += HandleQuestEvent;
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

    private void OnEnable()
    {
        RefreshNPCMarkers();
    }

    private void HandleQuestEvent(Quest quest) => RefreshNPCMarkers();
    private void HandleQuestEventWithStatus(PlayerQuestStatus status, Quest quest) => RefreshNPCMarkers();

    public void RefreshNPCMarkers()
    {
        // 기존에 지도 위에 생성되어 있던 마커 정리
        foreach (GameObject marker in activeMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        activeMarkers.Clear();

        if (mapController == null || mapContent == null || npcMarkerPrefab == null) return;

        // 현재 씬에 배치되어 있는 모든 NPC_QuestGiver를 찾아냄
        NPC_QuestGiver[] allNPCs = FindObjectsOfType<NPC_QuestGiver>();

        foreach (NPC_QuestGiver npc in allNPCs)
        {
            if (npc.questToGive == null) continue;

            // NPC가 가진 퀘스트의 현재 상태를 체크
            QuestStatus status = QuestManager.instance.GetQuestStatus(npc.questToGive.questID);
            Sprite targetSprite = null;

            if (status == QuestStatus.NOT_STARTED)
            {
                // 선행 퀘스트 완료 등으로 수락 가능해진 상태 포함 (!)
                targetSprite = iconAvailable;
            }
            else if (status == QuestStatus.COMPLETED)
            {
                // 목표를 다 깨서 NPC에게 보상을 받을 수 있는 상태 (?)
                targetSprite = iconComplete;
            }

            // 표시할 마커가 결정되면 동적으로 지도에 생성
            if (targetSprite != null)
            {
                GameObject newMarker = Instantiate(npcMarkerPrefab, mapContent);
                activeMarkers.Add(newMarker);

                Image markerImage = newMarker.GetComponent<Image>();
                if (markerImage != null) markerImage.sprite = targetSprite;

                RectTransform markerRect = newMarker.GetComponent<RectTransform>();
                if (markerRect != null)
                {
                    markerRect.anchorMin = new Vector2(0.5f, 0.5f);
                    markerRect.anchorMax = new Vector2(0.5f, 0.5f);
                    markerRect.pivot = new Vector2(0.5f, 0.5f);

                    Vector2 mapPos = mapController.GetMapPosition(npc.transform.position);
                    markerRect.anchoredPosition = mapPos;

                    UIMapInteractiveIcon interactScript = newMarker.GetComponent<UIMapInteractiveIcon>();
                    if (interactScript == null) interactScript = newMarker.AddComponent<UIMapInteractiveIcon>();

                    // 퀘스트 상태에 따라 텍스트 설명 다르게 세팅
                    string descText = (status == QuestStatus.NOT_STARTED) ?
                        $"[{npc.gameObject.name}]\n새로운 퀘스트가 있습니다." :
                        $"[{npc.gameObject.name}]\n퀘스트 완료 보고 가능!";

                    interactScript.Setup(descText, mapPos);
                }
            }
        }
    }
}