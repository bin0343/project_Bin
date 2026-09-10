using UnityEngine;
using System.Collections.Generic;

public class NPC_QuestGiver : Interactable
{
    [Header("NPC 정보")]
    public string npcName = "아린";

    [Header("보유 퀘스트")]
    public List<Quest> quests = new List<Quest>();

    [Header("말풍선 UI")]
    public GameObject bubble_Available;
    public GameObject bubble_InProgress;
    public GameObject bubble_Complete;

    protected override void Update()
    {
        // 대화 중에는 상호작용 UI 숨기기
        if (DialogueManager.instance != null && DialogueManager.instance.isDialogueActive)
        {
            if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
            return;
        }

        base.Update();
        UpdateQuestBubble();
    }

    private void UpdateQuestBubble()
    {
        Quest currentQuest = GetCurrentQuest();

        if (currentQuest == null || QuestManager.instance == null)
        {
            if (bubble_Available) bubble_Available.SetActive(false);
            if (bubble_InProgress) bubble_InProgress.SetActive(false);
            if (bubble_Complete) bubble_Complete.SetActive(false);
            return;
        }

        QuestStatus status = QuestManager.instance.GetQuestStatus(currentQuest.questID);

        bool canAccept = status == QuestStatus.NOT_STARTED && QuestManager.instance.CanAcceptQuest(currentQuest);

        if (bubble_Available) bubble_Available.SetActive(canAccept);

        if (bubble_InProgress) bubble_InProgress.SetActive(status == QuestStatus.IN_PROGRESS);

        if (bubble_Complete) bubble_Complete.SetActive(status == QuestStatus.COMPLETED);
    }

    protected override void OpenMenu()
    {
        Quest currentQuest = GetCurrentQuest();

        if (currentQuest == null || DialogueManager.instance == null || QuestManager.instance == null)
        {
            return;
        }

        QuestStatus status = QuestManager.instance.GetQuestStatus(currentQuest.questID);

        switch (status)
        {
            case QuestStatus.NOT_STARTED:
                if (!QuestManager.instance.CanAcceptQuest(currentQuest)) return;

                DialogueManager.instance.StartQuestSequence(npcName, currentQuest,
                    () =>
                    {
                        QuestManager.instance.AcceptQuest(currentQuest);

                        SetQuestTargetMarker(currentQuest);
                    });

                break;

            case QuestStatus.IN_PROGRESS:
                DialogueManager.instance.StartNormalSequence(npcName, currentQuest.inProgressDialogue);
                break;

            case QuestStatus.COMPLETED:
                DialogueManager.instance.StartNormalSequence(npcName, currentQuest.completeDialogue, () => {
                    QuestManager.instance.ClaimReward(currentQuest);
                });
                break;

            case QuestStatus.REWARD_CLAIMED:
                DialogueManager.instance.StartNormalSequence(npcName, currentQuest.afterCompleteDialogue);
                break;
        }
    }

    public Quest GetCurrentQuest()
    {
        if (QuestManager.instance == null) return null;

        // 1순위 : 목표를 끝내고 보상 보고하러 온 퀘스트
        foreach (Quest quest in quests)
        {
            if (quest == null) continue;

            if (QuestManager.instance.GetQuestStatus(quest.questID) == QuestStatus.COMPLETED)
            {
                return quest;
            }
        }

        // 2순위 : 지금 새로 받을 수 있는 퀘스트
        foreach (Quest quest in quests)
        {
            if (quest == null) continue;

            if (QuestManager.instance.CanAcceptQuest(quest))
            {
                return quest;
            }
        }

        // 3순위 : 이미 진행 중인 퀘스트
        foreach (Quest quest in quests)
        {
            if (quest == null) continue;

            if (QuestManager.instance.GetQuestStatus(quest.questID) == QuestStatus.IN_PROGRESS)
            {
                return quest;
            }
        }

        // 전부 끝난 경우 기존 afterCompleteDialogue를 유지하기 위한 처리
        for (int i = quests.Count - 1; i >= 0; i--)
        {
            Quest quest = quests[i];

            if (quest == null) continue;

            if (QuestManager.instance.GetQuestStatus(quest.questID) == QuestStatus.REWARD_CLAIMED)
            {
                return quest;
            }
        }

        return null;
    }

    private void SetQuestTargetMarker(Quest quest)
    {
        if (quest == null || QuestManager.instance == null) return;

        if (!QuestManager.instance.questLog.TryGetValue(quest.questID, out PlayerQuestStatus status))
        {
            return;
        }

        if (status.currentStepIndex < 0 || status.currentStepIndex >= quest.steps.Count)
        {
            return;
        }

        QuestStep currentStep = quest.steps[status.currentStepIndex];

        if (currentStep.objectives == null || currentStep.objectives.Count == 0)
        {
            return;
        }

        string targetID = currentStep.objectives[0].targetID;

        Transform destination = QuestTargetMarker.GetTarget(targetID);

        if (destination != null && QuestMarkerUI.instance != null)
        {
            QuestMarkerUI.instance.SetTarget(destination);

            Debug.Log($"[{targetID}] 현재 Quest Step 목표 마커 연결 성공!");
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (!other.CompareTag("Player")) return;

        if (interactionText == null || QuestManager.instance == null) return;

        Quest currentQuest = GetCurrentQuest();

        if (currentQuest == null) return;

        QuestStatus status = QuestManager.instance.GetQuestStatus(currentQuest.questID);

        // 모두 완료한 뒤의 일반 사후 대화라면
        // 기존 base의 "F : 대화" 문구를 그대로 사용
        if (status == QuestStatus.REWARD_CLAIMED) return;

        bool canInteractWithQuest = status != QuestStatus.NOT_STARTED || QuestManager.instance.CanAcceptQuest(currentQuest);

        if (canInteractWithQuest)
        {
            interactionText.text = $"{interactionKey} : {currentQuest.questTitle}";
        }
    }
}