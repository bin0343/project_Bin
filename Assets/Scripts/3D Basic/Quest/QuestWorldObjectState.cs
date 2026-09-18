using UnityEngine;

public class QuestWorldObjectState : MonoBehaviour
{
    [Header("Quest 조건")]
    public string questID;

    [Tooltip("이 Step에 도달했거나 지나갔으면 활성화")]
    public int activateFromStepIndex;

    [Header("제어할 오브젝트")]
    public GameObject targetObject;

    private void Start()
    {
        RefreshState();
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
            shouldBeActive = true;
        }

        targetObject.SetActive(shouldBeActive);

        if (LocalMapTeleportManager.instance != null)
        {
            LocalMapTeleportManager.instance.RefreshTeleportMarkers();
        }

        if (MiniMapTeleportManager.instance != null)
        {
            MiniMapTeleportManager.instance.RefreshMiniMapTeleports();
        }
    }
}