using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public Dictionary<string, PlayerQuestStatus> questLog = new Dictionary<string, PlayerQuestStatus>();
    private Dictionary<string, Quest> questDatabase;
    public List<PlayerQuestStatus> debug_QuestLogList = new List<PlayerQuestStatus>();

    // 현재 추적 중인 퀘스트 ID
    public string currentTrackedQuestID = "";

    public event Action<Quest> OnQuestAccepted;
    public event Action<PlayerQuestStatus, Quest> OnQuestProgressChanged;
    public event Action<PlayerQuestStatus, Quest> OnQuestCompleted;
    public event Action<Quest> OnQuestRewardClaimed;
    // 퀘스트 추적 대상이 바뀌었을 때 호출되는 이벤트
    public event Action<Quest> OnQuestTrackedChanged;

    private void Awake()
    {
        if (instance != null && instance != this) return;
        instance = this;

        questDatabase = new Dictionary<string, Quest>();

        Quest[] allQuests = Resources.LoadAll<Quest>("Data/Quests");

        foreach (Quest quest in allQuests)
        {
            if (questDatabase.ContainsKey(quest.questID)) continue;
            questDatabase.Add(quest.questID, quest);
        }
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            debug_QuestLogList = questLog.Values.ToList();
        }
    }

    public Quest GetQuestByID(string questID)
    {
        if (questDatabase.TryGetValue(questID, out Quest quest))
        {
            return quest;
        }
        Debug.LogWarning($"QuestDatabase에 ID가 없습니다: {questID}");
        return null;
    }

    public QuestStatus GetQuestStatus(string questID)
    {
        if (questLog.ContainsKey(questID))
        {
            return questLog[questID].status;
        }
        return QuestStatus.NOT_STARTED;
    }

    // 특정 퀘스트를 추적하도록 설정하는 함수
    public void SetTrackedQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID) || !questLog.ContainsKey(questID))
        {
            currentTrackedQuestID = "";
            OnQuestTrackedChanged?.Invoke(null); // 추적 해제
            return;
        }

        currentTrackedQuestID = questID;
        OnQuestTrackedChanged?.Invoke(GetQuestByID(questID));
        Debug.Log($"[{questID}] 퀘스트 추적 시작!");
    }

    public bool CanAcceptQuest(Quest quest)
    {
        if (quest == null) return false;

        if (GetQuestStatus(quest.questID) != QuestStatus.NOT_STARTED) return false;

        foreach (Quest prerequisite in quest.prerequisiteQuests)
        {
            if (prerequisite == null) continue;

            if (GetQuestStatus(prerequisite.questID) != QuestStatus.REWARD_CLAIMED)
            {
                return false;
            }
        }

        return true;
    }

    public void AcceptQuest(Quest quest)
    {
        if (!CanAcceptQuest(quest)) return;

        if (quest == null || questLog.ContainsKey(quest.questID)) return;

        PlayerQuestStatus newQuest = new PlayerQuestStatus(quest);
        questLog[quest.questID] = newQuest;
        Debug.Log($"퀘스트 수락: {quest.questTitle}");

        OnQuestAccepted?.Invoke(quest);

        // 편의성 기능: 퀘스트를 새로 받으면 자동으로 그것을 추적
        SetTrackedQuest(quest.questID);
    }

    public void AdvanceQuestProgress(string targetID, int amount)
    {
        foreach (var questStatus in questLog.Values.Where(q => q.status == QuestStatus.IN_PROGRESS))
        {
            if (questStatus.objectiveProgress.ContainsKey(targetID))
            {
                // 퀘스트 원본 데이터 가져오기
                if (!questDatabase.ContainsKey(questStatus.questID))
                {
                    Debug.LogWarning($"데이터베이스에 {questStatus.questID}가 없습니다!");
                    continue;
                }
                Quest originalQuest = questDatabase[questStatus.questID];

                if (questStatus.currentStepIndex < 0 || questStatus.currentStepIndex >= originalQuest.steps.Count)
                {
                    continue;
                }

                QuestStep currentStep = originalQuest.steps[questStatus.currentStepIndex];

                // 이 퀘스트의 해당 목표(objective) 찾기
                QuestObjective objective = currentStep.objectives.Find(o => o.targetID == targetID);
                if (objective == null) continue; // (이론상 발생 안 함)

                // 진행도 상승 (최대치를 넘지 않도록)
                questStatus.objectiveProgress[targetID] = Mathf.Min(
                    questStatus.objectiveProgress[targetID] + amount,
                    objective.requiredAmount
                );

                // 로그 수정 (이제 '?' 대신 'requiredAmount' 표시)
                Debug.Log($"퀘스트 진행: {questStatus.questID} - {targetID} ({questStatus.objectiveProgress[targetID]} / {objective.requiredAmount})");

                OnQuestProgressChanged?.Invoke(questStatus, originalQuest);
                // 이 퀘스트의 모든 목표가 달성되었는지 확인
                CheckCurrentStepCompletion(questStatus, originalQuest);
            }
        }
    }

    // 퀘스트 실패 처리 함수
    public void FailQuest(string questID)
    {
        if (questLog.ContainsKey(questID))
        {
            questLog[questID].status = QuestStatus.FAILED;

            // 알림용 이벤트 발생 (필요하면 UI_Toast 등으로 연결)
            Debug.Log($"[퀘스트 실패] 기한이 지나 퀘스트 '{questID}' 실패 처리됨.");

            // 만약 퀘스트 실패 시 UI를 갱신해야 한다면 호출
            // OnQuestProgressChanged?.Invoke(questLog[questID], questDatabase[questID]);
        }
    }

    private void CheckCurrentStepCompletion(PlayerQuestStatus status, Quest quest)
    {
        if (status.isWaitingForStepDialogue) return;

        if (status.status != QuestStatus.IN_PROGRESS) return;

        if (status.currentStepIndex < 0 || status.currentStepIndex >= quest.steps.Count) return;

        QuestStep currentStep = quest.steps[status.currentStepIndex];

        foreach (QuestObjective objective in currentStep.objectives)
        {
            if (!status.objectiveProgress.ContainsKey(objective.targetID) || status.objectiveProgress[objective.targetID] < objective.requiredAmount)
            {
                return;
            }
        }

        HandleStepCompleted(status, quest);
    }

    private void AdvanceToNextStep(PlayerQuestStatus status, Quest quest)
    {
        status.currentStepIndex++;

        // 아직 다음 단계가 존재
        if (status.currentStepIndex < quest.steps.Count)
        {
            status.InitializeCurrentStep(quest);

            Debug.Log($"[퀘스트 단계 진행] {quest.questTitle} " + $"→ Step {status.currentStepIndex + 1}");

            OnQuestProgressChanged?.Invoke(status, quest);
            return;
        }

        CompleteQuest(status, quest);
    }

    private void CompleteQuest(PlayerQuestStatus status, Quest quest)
    {
        status.status = QuestStatus.COMPLETED;

        OnQuestCompleted?.Invoke(status, quest);

        Debug.Log($"[퀘스트 완료] {quest.questTitle}");

        ClaimReward(quest);
    }

    private void HandleStepCompleted(PlayerQuestStatus status, Quest quest)
    {
        if (status.currentStepIndex < 0 || status.currentStepIndex >= quest.steps.Count)
        {
            return;
        }

        QuestStep currentStep = quest.steps[status.currentStepIndex];

        bool hasCompleteDialogue = currentStep.completeDialogue != null && currentStep.completeDialogue.Length > 0;

        if (hasCompleteDialogue && DialogueManager.instance != null)
        {
            status.isWaitingForStepDialogue = true;

            DialogueManager.instance.StartNormalSequence(currentStep.completeSpeakerName, currentStep.completeDialogue,
                () =>
                {
                    status.isWaitingForStepDialogue = false;
                    AdvanceToNextStep(status, quest);
                });

            return;
        }

        AdvanceToNextStep(status, quest);
    }

    public void ClaimReward(Quest quest)
    {
        if (quest == null || !questLog.ContainsKey(quest.questID)) return;
        if (questLog[quest.questID].status != QuestStatus.COMPLETED) return;

        if (Account_Manager.Instance != null)
        {
            Account_Manager.Instance.GainGold(quest.rewards.gold);
            Account_Manager.Instance.GainAccountExp(quest.rewards.experience); // 계정 레벨업
        }

        // 아이템 보상 (확장)
        /*if (quest.rewards.itemReward != null)
        {
            Player_Inventory.Instance.AddItem(quest.rewards.itemReward, 1);
        }*/

        questLog[quest.questID].status = QuestStatus.REWARD_CLAIMED;
        Debug.Log($"퀘스트 완료 및 보상 수령: {quest.questTitle}");
        // TODO: UI 갱신
        OnQuestRewardClaimed?.Invoke(quest);

        // 보상을 받은 퀘스트가 현재 추적 중인 퀘스트였다면 추적 해제
        if (currentTrackedQuestID == quest.questID)
        {
            SetTrackedQuest("");
        }
    }

    public List<QuestSaveData> GetQuestSaveData()
    {
        List<QuestSaveData> saveList = new List<QuestSaveData>();

        foreach (var kvp in questLog)
        {
            PlayerQuestStatus status = kvp.Value;
            QuestSaveData data = new QuestSaveData();

            data.questID = status.questID;
            data.status = (int)status.status; // Enum -> int 변환

            // 목표 진행도(Dictionary)를 리스트로 변환
            data.progressList = new List<QuestObjectiveSaveData>();
            foreach (var progressKvp in status.objectiveProgress)
            {
                QuestObjectiveSaveData objData = new QuestObjectiveSaveData();
                objData.targetID = progressKvp.Key;
                objData.count = progressKvp.Value;
                data.progressList.Add(objData);
            }

            saveList.Add(data);
        }
        return saveList;
    }

    // [추가] 데이터 불러오기 (List -> Dictionary)
    public void LoadQuestSaveData(List<QuestSaveData> savedList)
    {
        if (savedList == null) return;

        questLog.Clear(); // 기존 퀘스트 로그 초기화

        foreach (var data in savedList)
        {
            // 퀘스트 원본 데이터 찾기 (ScriptableObject)
            if (questDatabase.ContainsKey(data.questID))
            {
                Quest originalQuest = questDatabase[data.questID];

                // 플레이어 퀘스트 상태 복구
                PlayerQuestStatus newStatus = new PlayerQuestStatus(originalQuest);
                newStatus.status = (QuestStatus)data.status; // int -> Enum 복구

                // 진행도 복구
                foreach (var objData in data.progressList)
                {
                    if (newStatus.objectiveProgress.ContainsKey(objData.targetID))
                    {
                        newStatus.objectiveProgress[objData.targetID] = objData.count;
                    }
                }

                // 로그에 추가
                questLog.Add(data.questID, newStatus);
            }
            else
            {
                Debug.LogError($"[QuestManager] 로드 실패! ID '{data.questID}'를 DB에서 찾을 수 없습니다. Resources/Data/Quests 폴더와 ScriptableObject의 QuestID를 확인하세요.");
            }
        }

        Debug.Log($"퀘스트 로드 완료: {questLog.Count}개");

        // (선택사항) 로드 후 UI 추적기가 있다면 갱신하라고 알리기
        // OnQuestProgressChanged?.Invoke(...) 등을 호출하거나 UI_QuestTracker에서 Refresh
    }
}
