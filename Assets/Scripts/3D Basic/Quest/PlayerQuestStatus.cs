using System.Collections.Generic;

public enum QuestStatus
{
    NOT_STARTED,
    IN_PROGRESS,
    COMPLETED,      //완료지만 보상 안받음
    REWARD_CLAIMED,  //보상까지 다 받음
    FAILED
}

[System.Serializable]
public class PlayerQuestStatus
{
    public string questID;
    public QuestStatus status;

    public int currentStepIndex;
    public Dictionary<string, int> objectiveProgress;

    public bool isWaitingForStepDialogue;

    public PlayerQuestStatus(Quest quest)
    {
        questID = quest.questID;
        status = QuestStatus.IN_PROGRESS;

        currentStepIndex = 0;
        isWaitingForStepDialogue = false;
        objectiveProgress = new Dictionary<string, int>();

        InitializeCurrentStep(quest);
    }

    public void InitializeCurrentStep(Quest quest)
    {
        objectiveProgress.Clear();

        if (quest == null)
            return;

        if (currentStepIndex < 0 || currentStepIndex >= quest.steps.Count)
            return;

        QuestStep currentStep = quest.steps[currentStepIndex];

        foreach (QuestObjective objective in currentStep.objectives)
        {
            objectiveProgress[objective.targetID] = 0;
        }
    }
}
