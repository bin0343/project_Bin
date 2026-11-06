using UnityEngine;
using UnityEngine.Events;

public class QuestCompletionStateApplier : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string questID;

    [Header("Quest 완료 시 적용할 영구 상태")]
    [SerializeField]
    private UnityEvent onQuestFinished;

    private bool appliedThisSession;

    private void Start()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void HandleQuestCompleted(PlayerQuestStatus status, Quest quest)
    {
        if (status == null || status.questID != questID) return;

        Apply();
    }

    public void RefreshState()
    {
        if (QuestManager.instance == null) return;

        QuestStatus status = QuestManager.instance.GetQuestStatus(questID);

        if (status == QuestStatus.COMPLETED || status == QuestStatus.REWARD_CLAIMED)
        {
            Apply();
        }
    }

    private void Apply()
    {
        if (appliedThisSession) return;

        appliedThisSession = true;

        Debug.Log($"[QuestCompletionState] {questID} 완료 상태 적용");

        onQuestFinished?.Invoke();
    }

    public static void RefreshAll()
    {
        QuestCompletionStateApplier[] appliers = FindObjectsOfType<QuestCompletionStateApplier>(true);

        foreach (QuestCompletionStateApplier applier in appliers)
        {
            if (applier == null) continue;

            applier.RefreshState();
        }
    }
}