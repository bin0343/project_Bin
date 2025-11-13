using System.Collections.Generic;

public enum QuestStatus
{
    NOT_STARTED,
    IN_PROGRESS,
    COMPLETED,      //완료지만 보상 안받음
    REWARD_CLAIMED  //보상까지 다 받음
}

[System.Serializable]
public class PlayerQuestStatus
{
    public string questID;
    public QuestStatus status;

    // 퀘스트의 목표별 현재 진행도 (ex: "MON_Rat" -> 3마리 잡음)
    public Dictionary<string, int> objectiveProgress;

    public PlayerQuestStatus(Quest quest)
    {
        questID = quest.questID;
        status = QuestStatus.IN_PROGRESS;
        objectiveProgress = new Dictionary<string, int>();
        foreach (var obj in quest.objectives)
        {
            objectiveProgress[obj.targetID] = 0;
        }
    }
}
