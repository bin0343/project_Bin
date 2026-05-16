using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestEventTrigger : Interactable
{
    [Header("퀘스트 목표")]
    public string targetQuestID;        
    public string targetObjectiveID;

    [Header("퀘스트 완료 시 출력 대화")]
    public string npcName = "npc이름";
    [TextArea(2, 4)] public string successDialogue = "성공 시 대화";

    [Header("상호 작용 시의 맵 이벤트")]  //ex)몬스터 스폰
    public UnityEvent onQuestEventTriggered;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other.CompareTag("Player"))
        {
            QuestStatus status = QuestManager.instance.GetQuestStatus(targetQuestID);
            if (status == QuestStatus.IN_PROGRESS)
            {
                if (interactionText != null) interactionText.text = $"{interactionKey} : 포획하기";
            }
            else
            {
                if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
            }
        }
    }

    protected override void OpenMenu()
    {
        QuestStatus status = QuestManager.instance.GetQuestStatus(targetQuestID);

        if (status == QuestStatus.IN_PROGRESS)
        {
            QuestManager.instance.AdvanceQuestProgress(targetQuestID, 1);

            if (DialogueManager.instance != null && !string.IsNullOrEmpty(successDialogue))
            {
                DialogueManager.instance.StartNormalSequence(npcName, new string[] { successDialogue });
            }

            onQuestEventTriggered?.Invoke();

            gameObject.SetActive(false);
        }

        isMenuOpen = false;
        if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
    }
}
