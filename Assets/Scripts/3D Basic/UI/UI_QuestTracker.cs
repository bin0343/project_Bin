using UnityEngine;

public class UI_QuestTracker : MonoBehaviour
{
    public static UI_QuestTracker instance;

    private bool isTemporaryOverride;
    private bool isPrimaryInstance;

    public GameObject questItemPrefab;
    public Transform questListContainer;

    private GameObject currentTrackedGO;
    private UI_QuestTrackerItem currentTrackedUI;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            isPrimaryInstance = false;
            return;
        }

        instance = this;
        isPrimaryInstance = true;
    }

    void Start()
    {
        if (!isPrimaryInstance) return;

        if (QuestManager.instance == null) return;

        QuestManager.instance.OnQuestTrackedChanged += HandleQuestTrackedChanged;
        QuestManager.instance.OnQuestProgressChanged += HandleQuestProgressChanged;

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
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    private void HandleQuestTrackedChanged(Quest quest)
    {
        if (isTemporaryOverride) return;

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
        if (isTemporaryOverride) return;

        // 업데이트된 퀘스트가 "현재 내가 추적 중인 퀘스트"일 때만 UI 갱신
        if (QuestManager.instance.currentTrackedQuestID == status.questID && currentTrackedUI != null)
        {
            currentTrackedUI.UpdateProgress(status, quest);
        }
    }

    public void BeginTemporaryObjective(string objectiveName, int currentAmount, int requiredAmount)
    {
        isTemporaryOverride = true;

        if (currentTrackedGO != null)
        {
            Destroy(currentTrackedGO);

            currentTrackedGO = null;
            currentTrackedUI = null;
        }

        currentTrackedGO = Instantiate(questItemPrefab, questListContainer);

        currentTrackedUI = currentTrackedGO.GetComponent<UI_QuestTrackerItem>();

        if (currentTrackedUI != null)
        {
            currentTrackedUI.SetupTemporaryObjective(objectiveName, currentAmount, requiredAmount);
        }
    }

    public void UpdateTemporaryObjective(int currentAmount, int requiredAmount)
    {
        if (!isTemporaryOverride || currentTrackedUI == null) return;

        currentTrackedUI.UpdateTemporaryObjective(currentAmount, requiredAmount);
    }

    public void EndTemporaryObjective()
    {
        isTemporaryOverride = false;

        if (currentTrackedGO != null)
        {
            Destroy(currentTrackedGO);

            currentTrackedGO = null;
            currentTrackedUI = null;
        }

        if (QuestManager.instance == null) return;

        string trackedQuestID = QuestManager.instance.currentTrackedQuestID;

        if (string.IsNullOrEmpty(trackedQuestID)) return;

        Quest quest = QuestManager.instance.GetQuestByID(trackedQuestID);

        HandleQuestTrackedChanged(quest);
    }

    public void HideTemporaryObjective()
    {
        if (!isTemporaryOverride) return;

        if (currentTrackedGO != null)
        {
            Destroy(currentTrackedGO);

            currentTrackedGO = null;
            currentTrackedUI = null;
        }
    }
}