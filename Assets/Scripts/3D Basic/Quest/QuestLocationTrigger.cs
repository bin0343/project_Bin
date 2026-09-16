using UnityEngine;

public class QuestLocationTrigger : MonoBehaviour
{
    [Header("퀘스트 목표 ID")]
    public string targetID;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (QuestManager.instance == null) return;

        QuestManager.instance.AdvanceQuestProgress(targetID, 1);
    }
}