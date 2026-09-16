using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestEventTrigger : Interactable
{
    [Header("퀘스트 목표")]
    public string targetQuestID;        
    public string targetObjectiveID;

    [Header("상호작용 설정")]
    public string interactionLabel = "상호작용";

    [Tooltip("상호작용 성공 후 이 오브젝트를 비활성화할지")]
    public bool disableAfterTrigger = false;

    [Header("상호 작용 시의 맵 이벤트")]  //ex)몬스터 스폰
    public UnityEvent onQuestEventTriggered;

    private bool isProcessingEvent = false;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (!other.CompareTag("Player")) return;

        if (IsObjectiveActive())
        {
            if (interactionText != null)
            {
                interactionText.text = $"{interactionKey} : {interactionLabel}";
            }
        }
        else
        {
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }
    }

    protected override void OpenMenu()
    {
        if (isProcessingEvent || !IsObjectiveActive())
        {
            isMenuOpen = false;

            if (interactionPromptUI != null) interactionPromptUI.SetActive(false);

            return;
        }

        isProcessingEvent = true;

        // Quest 진행
        QuestManager.instance.AdvanceQuestProgress(targetObjectiveID, 1);

        isMenuOpen = false;

        if (interactionPromptUI != null) interactionPromptUI.SetActive(false);

        // 즉시 UnityEvent 실행하지 않고 완료 대사가 끝날 때까지 기다림
        StartCoroutine(InvokeQuestEventAfterDialogue());
    }

    private bool IsObjectiveActive()
    {
        if (QuestManager.instance == null) return false;

        return QuestManager.instance.IsCurrentObjective(targetQuestID, targetObjectiveID);
    }

    private IEnumerator InvokeQuestEventAfterDialogue()
    {
        // Step 완료 대사가 진행 중이라면 실제로 대화창이 닫힐 때까지 기다림
        if (DialogueManager.instance != null)
        {
            while (DialogueManager.instance.isDialogueActive)
            {
                yield return null;
            }
        }

        // 이제 월드 이벤트 실행
        onQuestEventTriggered?.Invoke();

        // 이벤트에서 새 QuestTargetMarker가 활성화됐을 수도 있으므로 현재 Quest 마커를 한 번 다시 찾음
        if (QuestManager.instance != null)
        {
            QuestManager.instance.RefreshTrackedQuestTarget();
        }

        if (disableAfterTrigger)
        {
            gameObject.SetActive(false);
            yield break;
        }

        isProcessingEvent = false;
    }
}
