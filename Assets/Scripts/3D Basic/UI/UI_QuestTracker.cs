using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestTracker : MonoBehaviour
{
    [Tooltip("개별 퀘스트 UI 항목 프리팹")]
    public GameObject questItemPrefab;

    [Tooltip("프리팹이 생성될 부모 Transform")]
    public Transform questListContainer;

    private Dictionary<string, UI_QuestTrackerItem> activeQuestUIs = new Dictionary<string, UI_QuestTrackerItem>();

    void Start()
    {
        if (QuestManager.instance == null)
        {
            Debug.LogError("QuestManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        QuestManager.instance.OnQuestAccepted += HandleQuestAccepted;
        QuestManager.instance.OnQuestProgressChanged += HandleQuestProgressChanged;
        QuestManager.instance.OnQuestCompleted += HandleQuestCompleted;
        QuestManager.instance.OnQuestRewardClaimed += HandleQuestRewardClaimed;

        PopulateInitialQuests();    //게임 시작 시 진행 중 퀘스트 목록 불러오기
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 구독 해제
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestAccepted -= HandleQuestAccepted;
            QuestManager.instance.OnQuestProgressChanged -= HandleQuestProgressChanged;
            QuestManager.instance.OnQuestCompleted -= HandleQuestCompleted;
            QuestManager.instance.OnQuestRewardClaimed -= HandleQuestRewardClaimed;
        }
    }

    private void PopulateInitialQuests()
    {
        foreach (var kvp in QuestManager.instance.questLog)
        {
            PlayerQuestStatus status = kvp.Value;
            if (status.status == QuestStatus.IN_PROGRESS || status.status == QuestStatus.COMPLETED)
            {
                Quest quest = QuestManager.instance.GetQuestByID(status.questID);
                if (quest != null)
                {
                    AddQuestUI(quest, status);
                }
            }
        }
    }

    private void HandleQuestAccepted(Quest quest)
    {
        PlayerQuestStatus status = QuestManager.instance.questLog[quest.questID];
        AddQuestUI(quest, status);
    }

    private void AddQuestUI(Quest quest, PlayerQuestStatus status)
    {
        if (activeQuestUIs.ContainsKey(quest.questID)) return;

        GameObject questItemGO = Instantiate(questItemPrefab, questListContainer);

        //새 퀘스트를 목록 최상단(0번 인덱스)으로 이동
        questItemGO.transform.SetAsFirstSibling();

        UI_QuestTrackerItem uiItem = questItemGO.GetComponent<UI_QuestTrackerItem>();
        uiItem.Setup(quest, status);    //UI 내용 설정

        if (status.status == QuestStatus.COMPLETED)     //불러온 퀘스트가 이미 완료 상태라면
        {
            uiItem.SetCompletedVisuals();
        }

        activeQuestUIs[quest.questID] = uiItem;
    }
    
    private void HandleQuestProgressChanged(PlayerQuestStatus status, Quest quest)
    {
        if (activeQuestUIs.TryGetValue(status.questID, out UI_QuestTrackerItem uiItem))
        {
            uiItem.UpdateProgress(status, quest);
        }
    }

    private void HandleQuestCompleted(PlayerQuestStatus status, Quest quest)
    {
        if (activeQuestUIs.TryGetValue(status.questID, out UI_QuestTrackerItem uiItem))
        {
            uiItem.SetCompletedVisuals(); // UI 완료 상태로 변경 (예: "(완료)" 표시)
        }
    }

    private void HandleQuestRewardClaimed(Quest quest)
    {
        if (activeQuestUIs.TryGetValue(quest.questID, out UI_QuestTrackerItem uiItem))
        {
            Destroy(uiItem.gameObject); // UI 항목 제거
            activeQuestUIs.Remove(quest.questID); // 딕셔너리에서 제거
        }
    }
}
