using UnityEngine;

public class NPC_QuestGiver : Interactable
{
    [Header("NPC 정보")]
    public string npcName = "아린";
    public Quest questToGive;

    [Header("말풍선 UI")]
    public GameObject bubble_Available;
    public GameObject bubble_InProgress;
    public GameObject bubble_Complete;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        // 접근 시 상호작용 텍스트를 "F : 퀘스트 제목"으로 변경
        if (other.CompareTag("Player") && questToGive != null && interactionText != null)
        {
            interactionText.text = $"{interactionKey} : {questToGive.questTitle}";
        }
    }

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
        if (questToGive == null) return;
        QuestStatus status = QuestManager.instance.GetQuestStatus(questToGive.questID);

        if (bubble_Available) bubble_Available.SetActive(status == QuestStatus.NOT_STARTED);
        if (bubble_InProgress) bubble_InProgress.SetActive(status == QuestStatus.IN_PROGRESS);
        if (bubble_Complete) bubble_Complete.SetActive(status == QuestStatus.COMPLETED);
    }

    protected override void OpenMenu()
    {
        if (questToGive == null || DialogueManager.instance == null) return;

        QuestStatus status = QuestManager.instance.GetQuestStatus(questToGive.questID);

        switch (status)
        {
            case QuestStatus.NOT_STARTED:
                // 시작 대화 + 선택지 모드
                DialogueManager.instance.StartQuestSequence(npcName, questToGive, () => 
                {
                    QuestManager.instance.AcceptQuest(questToGive);

                    if (questToGive.objectives.Count > 0)
                    {
                        string targetID = questToGive.objectives[0].targetID;

                        // 명부에서 해당 ID를 가진 오브젝트를 찾음
                        Transform autoDestination = QuestTargetMarker.GetTarget(targetID);

                        if (autoDestination != null && QuestMarkerUI.instance != null)
                        {
                            QuestMarkerUI.instance.SetTarget(autoDestination);
                            Debug.Log($"[{targetID}] 자동 마커 연결 성공!");
                        }
                        else
                        {
                            Debug.LogWarning($"씬에 '{targetID}' ID를 가진 QuestTargetMarker가 없습니다.");
                        }
                    }
                });
                break;

            case QuestStatus.IN_PROGRESS:
                DialogueManager.instance.StartNormalSequence(npcName, questToGive.inProgressDialogue);
                break;

            case QuestStatus.COMPLETED:
                DialogueManager.instance.StartNormalSequence(npcName, questToGive.completeDialogue, () => {
                    QuestManager.instance.ClaimReward(questToGive);
                });
                break;

            case QuestStatus.REWARD_CLAIMED:
                DialogueManager.instance.StartNormalSequence(npcName, questToGive.afterCompleteDialogue);
                break;
        }
    }
}