using UnityEngine;

public class UI_QuestTracker : MonoBehaviour
{
    public GameObject questItemPrefab;
    public Transform questListContainer;

    private GameObject currentTrackedGO;
    private UI_QuestTrackerItem currentTrackedUI;

    void Start()
    {
        if (QuestManager.instance == null) return;

        QuestManager.instance.OnQuestTrackedChanged += HandleQuestTrackedChanged;
        QuestManager.instance.OnQuestProgressChanged += HandleQuestProgressChanged;
        QuestManager.instance.OnQuestCompleted += HandleQuestCompleted;
        QuestManager.instance.OnQuestRewardClaimed += HandleQuestRewardClaimed;

        // 게임 시작 시 이미 추적 중인 퀘스트가 있다면 띄우기
        if (!string.IsNullOrEmpty(QuestManager.instance.currentTrackedQuestID))
        {
            Quest q = QuestManager.instance.GetQuestByID(QuestManager.instance.currentTrackedQuestID);
            HandleQuestTrackedChanged(q);
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestTrackedChanged -= HandleQuestTrackedChanged;
            QuestManager.instance.OnQuestProgressChanged -= HandleQuestProgressChanged;
            QuestManager.instance.OnQuestCompleted -= HandleQuestCompleted;
            QuestManager.instance.OnQuestRewardClaimed -= HandleQuestRewardClaimed;
        }
    }

    private void HandleQuestTrackedChanged(Quest quest)
    {
        // 1. 기존에 떠 있던 UI 삭제
        if (currentTrackedGO != null)
        {
            Destroy(currentTrackedGO);
            currentTrackedUI = null;
        }

        // 2. 추적 취소(null) 상태면 그냥 종료
        if (quest == null) return;

        // 3. 새 퀘스트 UI 생성
        PlayerQuestStatus status = QuestManager.instance.questLog[quest.questID];
        currentTrackedGO = Instantiate(questItemPrefab, questListContainer);
        currentTrackedUI = currentTrackedGO.GetComponent<UI_QuestTrackerItem>();
        currentTrackedUI.Setup(quest, status);

        if (status.status == QuestStatus.COMPLETED)
        {
            currentTrackedUI.SetCompletedVisuals();
        }
    }

    private void HandleQuestProgressChanged(PlayerQuestStatus status, Quest quest)
    {
        // 업데이트된 퀘스트가 "현재 내가 추적 중인 퀘스트"일 때만 UI 갱신
        if (QuestManager.instance.currentTrackedQuestID == status.questID && currentTrackedUI != null)
        {
            currentTrackedUI.UpdateProgress(status, quest);
        }
    }

    private void HandleQuestCompleted(PlayerQuestStatus status, Quest quest)
    {
        if (QuestManager.instance.currentTrackedQuestID == status.questID && currentTrackedUI != null)
        {
            currentTrackedUI.SetCompletedVisuals();
        }
    }

    private void HandleQuestRewardClaimed(Quest quest)
    {
        // 보상을 받은 게 내가 추적하던 퀘스트라면, 삭제(Track 취소는 QuestManager가 알아서 던져줌)
        if (QuestManager.instance.currentTrackedQuestID == quest.questID && currentTrackedGO != null)
        {
            Destroy(currentTrackedGO);
            currentTrackedUI = null;
        }
    }
}