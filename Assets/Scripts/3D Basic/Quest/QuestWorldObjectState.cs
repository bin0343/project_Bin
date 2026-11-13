using UnityEngine;

public class QuestWorldObjectState : MonoBehaviour
{
    [Header("Quest 조건")]
    public string questID;

    [Tooltip("이 Step에 도달했거나 지나갔으면 활성화")]
    public int activateFromStepIndex;

    [Header("제어할 오브젝트")]
    public GameObject targetObject;

    [Header("완료 후 상태")]
    [Tooltip("퀘스트를 완료한 뒤에도 이 오브젝트를 유지할지")]
    public bool keepActiveAfterQuestCompletion = false;

    [Tooltip("-1이면 진행 중 계속 유지. 값이 있으면 이 Step부터 비활성화")]
    public int deactivateAtStepIndex = -1;

    [Header("지도 마커 연동")]
    [SerializeField] private bool syncMapRevealWithObjectState = false;

    [SerializeField] private MapLocationMarker mapLocationMarker;

    private void Start()
    {
        RefreshState();

        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestAccepted += HandleQuestAccepted;
            QuestManager.instance.OnQuestProgressChanged += HandleQuestProgressChanged;
            QuestManager.instance.OnQuestCompleted += HandleQuestCompleted;
            QuestManager.instance.OnQuestRewardClaimed += HandleQuestRewardClaimed;
        }

    }

    public void RefreshState()
    {
        if (targetObject == null || QuestManager.instance == null)
        {
            return;
        }

        if (!QuestManager.instance.questLog.TryGetValue(questID, out PlayerQuestStatus status))
        {
            targetObject.SetActive(false);
            return;
        }

        bool shouldBeActive = status.status == QuestStatus.IN_PROGRESS && status.currentStepIndex >= activateFromStepIndex;

        if (status.status == QuestStatus.COMPLETED || status.status == QuestStatus.REWARD_CLAIMED)
        {
            shouldBeActive = keepActiveAfterQuestCompletion;
        }

        if (status.status == QuestStatus.IN_PROGRESS)
        {
            bool reachedStart = status.currentStepIndex >= activateFromStepIndex;

            bool beforeEnd = deactivateAtStepIndex < 0 || status.currentStepIndex < deactivateAtStepIndex;

            shouldBeActive = status.status == QuestStatus.IN_PROGRESS && reachedStart && beforeEnd;
        }

        targetObject.SetActive(shouldBeActive);

        if (syncMapRevealWithObjectState && mapLocationMarker != null)
        {
            mapLocationMarker.isRevealed = shouldBeActive;
        }

        LocalMapLocationManager localLocationManager = FindObjectOfType<LocalMapLocationManager>(true);

        if (localLocationManager != null)
        {
            localLocationManager.RefreshLocationMarkers();
        }

        MiniMapLocationManager miniLocationManager = FindObjectOfType<MiniMapLocationManager>(true);

        if (miniLocationManager != null)
        {
            miniLocationManager.RefreshLocationMarkers();
        }

        if (LocalMapTeleportManager.instance != null)
        {
            LocalMapTeleportManager.instance.RefreshTeleportMarkers();
        }

        if (MiniMapTeleportManager.instance != null)
        {
            MiniMapTeleportManager.instance.RefreshMiniMapTeleports();
        }
    }

    private void HandleQuestAccepted(Quest quest)
    {
        if (quest == null || quest.questID != questID) return;

        RefreshState();
    }

    private void HandleQuestProgressChanged(PlayerQuestStatus status, Quest quest)
    {
        if (status == null || status.questID != questID) return;

        RefreshState();
    }

    private void HandleQuestCompleted(PlayerQuestStatus status, Quest quest)
    {
        if (status == null || status.questID != questID) return;

        RefreshState();
    }

    private void HandleQuestRewardClaimed(Quest quest)
    {
        if (quest == null || quest.questID != questID) return;

        RefreshState();
    }

    private void OnDestroy()
    {
        if (QuestManager.instance == null) return;

        QuestManager.instance.OnQuestAccepted -= HandleQuestAccepted;
        QuestManager.instance.OnQuestProgressChanged -= HandleQuestProgressChanged;
        QuestManager.instance.OnQuestCompleted -= HandleQuestCompleted;
        QuestManager.instance.OnQuestRewardClaimed -= HandleQuestRewardClaimed;
    }
}