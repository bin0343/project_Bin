using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class QuestLocationTrigger : MonoBehaviour
{
    [Header("퀘스트 목표")]
    public string targetQuestID;
    public string targetObjectiveID;

    [Header("목표 도착 후 이벤트")]
    public UnityEvent onLocationReached;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (hasTriggered) return;

        if (QuestManager.instance == null) return;

        if (!QuestManager.instance.IsCurrentObjective(targetQuestID, targetObjectiveID))
        {
            return;
        }

        hasTriggered = true;

        QuestManager.instance.AdvanceQuestProgress(targetObjectiveID, 1);

        StartCoroutine(InvokeEventAfterDialogue());
    }

    private IEnumerator InvokeEventAfterDialogue()
    {
        if (DialogueManager.instance != null)
        {
            while (DialogueManager.instance.isDialogueActive)
            {
                yield return null;
            }
        }

        onLocationReached?.Invoke();
    }
}