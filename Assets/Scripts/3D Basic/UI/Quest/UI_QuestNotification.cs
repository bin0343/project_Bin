using UnityEngine;
using TMPro;
using DG.Tweening;

public class UI_QuestNotification : MonoBehaviour
{
    [Header("UI")]
    public GameObject notificationPanel;
    public CanvasGroup notificationCanvasGroup;
    public TMP_Text headerText;
    public TMP_Text contentText;

    [Header("연출 시간")]
    public float fadeInDuration = 0.25f;
    public float displayDuration = 2.0f;
    public float fadeOutDuration = 0.35f;

    private Sequence notificationSequence;

    private void Start()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestAccepted += HandleQuestAccepted;
            QuestManager.instance.OnQuestStepChanged += HandleStepChanged;
            QuestManager.instance.OnQuestCompleted += HandleQuestCompleted;
        }

        if (notificationCanvasGroup != null)
        {
            notificationCanvasGroup.alpha = 0f;
        }

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnQuestAccepted -= HandleQuestAccepted;
            QuestManager.instance.OnQuestStepChanged -= HandleStepChanged;
            QuestManager.instance.OnQuestCompleted -= HandleQuestCompleted;
        }

        if (notificationSequence != null && notificationSequence.IsActive())
        {
            notificationSequence.Kill();
        }
    }

    private void HandleQuestAccepted(Quest quest)
    {
        if (quest == null) return;

        string objectiveText = "";

        if (quest.steps != null && quest.steps.Count > 0 && quest.steps[0].objectives != null && quest.steps[0].objectives.Count > 0)
        {
            QuestObjective objective = quest.steps[0].objectives[0];

            objectiveText = string.IsNullOrWhiteSpace(objective.displayText) ? objective.targetID : objective.displayText;
        }

        ShowNotification("신규 퀘스트 수락", $"{quest.questTitle}");
    }

    private void HandleStepChanged(PlayerQuestStatus status, Quest quest)
    {
        if (status == null || quest == null) return;

        if (status.currentStepIndex < 0 || status.currentStepIndex >= quest.steps.Count)
        {
            return;
        }

        QuestStep step = quest.steps[status.currentStepIndex];

        string objectiveText = "";

        if (step.objectives != null && step.objectives.Count > 0)
        {
            QuestObjective objective = step.objectives[0];

            objectiveText = string.IsNullOrWhiteSpace(objective.displayText) ? objective.targetID : objective.displayText;
        }

        ShowNotification("목표 갱신", objectiveText);
    }

    private void HandleQuestCompleted(PlayerQuestStatus status, Quest quest)
    {
        if (quest == null) return;

        ShowNotification("퀘스트 완료", quest.questTitle);
    }

    private void ShowNotification(string header, string content)
    {
        if (notificationPanel == null || notificationCanvasGroup == null)
        {
            return;
        }

        // 이전 알림이 아직 재생 중이면 중단
        if (notificationSequence != null && notificationSequence.IsActive())
        {
            notificationSequence.Kill();
        }

        if (headerText != null)
        {
            headerText.text = header;
        }

        if (contentText != null)
        {
            contentText.text = content;
        }

        notificationPanel.SetActive(true);

        // 새 알림은 다시 투명 상태에서 시작
        notificationCanvasGroup.alpha = 0f;

        notificationSequence = DOTween.Sequence();

        notificationSequence
            .Append(notificationCanvasGroup.DOFade(1f, fadeInDuration))
            .AppendInterval(displayDuration)
            .Append(notificationCanvasGroup.DOFade(0f, fadeOutDuration))
            .OnComplete(() =>
            {
                notificationPanel.SetActive(false);
                notificationSequence = null;
            }).SetUpdate(true);
    }
}