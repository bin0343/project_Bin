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

    public event Action<Quest> OnQuestAccepted;
    public event Action<PlayerQuestStatus, Quest> OnQuestProgressChanged;
    public event Action<PlayerQuestStatus, Quest> OnQuestCompleted;
    public event Action<Quest> OnQuestRewardClaimed;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        questDatabase = new Dictionary<string, Quest>();

        Quest[] allQuests = Resources.LoadAll<Quest>("Data/Quests");

        foreach (Quest quest in allQuests)
        {
            if (questDatabase.ContainsKey(quest.questID))
            {
                Debug.LogWarning($"중복된 퀘스트 ID가 있습니다: {quest.questID}");
                continue;
            }
            questDatabase.Add(quest.questID, quest);
        }
        Debug.Log($"퀘스트 {allQuests.Length}개를 데이터베이스에 로드했습니다.");
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

    public void AcceptQuest(Quest quest)
    {
        if (quest == null || questLog.ContainsKey(quest.questID)) return;

        PlayerQuestStatus newQuest = new PlayerQuestStatus(quest);
        questLog[quest.questID] = newQuest;
        Debug.Log($"퀘스트 수락: {quest.questTitle}");

        OnQuestAccepted?.Invoke(quest);
    }

    public void AdvanceQuestProgress(string targetID, int amount)
    {
        foreach (var questStatus in questLog.Values.Where(q => q.status == QuestStatus.IN_PROGRESS))
        {
            if (questStatus.objectiveProgress.ContainsKey(targetID))
            {
                // 1. 퀘스트 원본 데이터 가져오기
                if (!questDatabase.ContainsKey(questStatus.questID))
                {
                    Debug.LogWarning($"데이터베이스에 {questStatus.questID}가 없습니다!");
                    continue;
                }
                Quest originalQuest = questDatabase[questStatus.questID];

                // 2. 이 퀘스트의 해당 목표(objective) 찾기
                QuestObjective objective = originalQuest.objectives.Find(o => o.targetID == targetID);
                if (objective == null) continue; // (이론상 발생 안 함)

                // 3. 진행도 상승 (최대치를 넘지 않도록)
                questStatus.objectiveProgress[targetID] = Mathf.Min(
                    questStatus.objectiveProgress[targetID] + amount,
                    objective.requiredAmount
                );

                // 4. 로그 수정 (이제 '?' 대신 'requiredAmount' 표시)
                Debug.Log($"퀘스트 진행: {questStatus.questID} - {targetID} ({questStatus.objectiveProgress[targetID]} / {objective.requiredAmount})");

                OnQuestProgressChanged?.Invoke(questStatus, originalQuest);
                // 5. 이 퀘스트의 모든 목표가 달성되었는지 확인
                CheckQuestCompletion(questStatus, originalQuest);
            }
        }
    }

    private void CheckQuestCompletion(PlayerQuestStatus status, Quest quest)
    {
        // 이미 완료 상태이거나 진행 중이 아니면 체크할 필요 없음
        if (status.status != QuestStatus.IN_PROGRESS) return;

        bool allObjectivesMet = true;

        // 퀘스트의 '모든' 목표를 순회
        foreach (var objective in quest.objectives)
        {
            // 플레이어의 진행도(objectiveProgress)가 목표치(requiredAmount)보다 적으면
            if (!status.objectiveProgress.ContainsKey(objective.targetID) ||
                status.objectiveProgress[objective.targetID] < objective.requiredAmount)
            {
                // 아직 덜 끝남
                allObjectivesMet = false;
                break;
            }
        }

        // 'allObjectivesMet'가 true로 유지되었다면 (모든 목표를 달성했다면)
        if (allObjectivesMet)
        {
            // 상태를 'COMPLETED'로 변경!
            status.status = QuestStatus.COMPLETED;

            OnQuestCompleted?.Invoke(status, quest);

            Debug.Log($"퀘스트 목표 달성: {quest.questTitle}! NPC에게 돌아가 보상을 받으세요.");
            // TODO: 퀘스트 로그 UI 갱신, NPC 머리 위에 '?' 아이콘 띄우기 등
        }
    }

    public void ClaimReward(Quest quest)
    {
        if (quest == null || !questLog.ContainsKey(quest.questID)) return;
        if (questLog[quest.questID].status != QuestStatus.COMPLETED) return;

        Player_Stat playerStat = Player_Inventory.instance.GetComponent<Player_Stat>();
        if (playerStat != null)
        {
            playerStat.gold += quest.rewards.gold;
            playerStat.GainExp(quest.rewards.experience);
        }

        // 아이템 보상 (확장)
        /*if (quest.rewards.itemReward != null)
        {
            Player_Inventory.instance.AddItem(quest.rewards.itemReward, 1);
        }*/

        questLog[quest.questID].status = QuestStatus.REWARD_CLAIMED;
        Debug.Log($"퀘스트 완료 및 보상 수령: {quest.questTitle}");
        // TODO: UI 갱신
        OnQuestRewardClaimed?.Invoke(quest);
    }
}
