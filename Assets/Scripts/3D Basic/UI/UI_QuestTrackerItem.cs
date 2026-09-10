using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_QuestTrackerItem : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Text titleText;
    public Transform objectivesContainer;
    public GameObject objectiveTextPrefab;

    [Header("완료 시 시각 효과")]
    public Color completedColor = Color.green;
    public Color normalColor = Color.white;

    // 빠른 접근을 위해 목표(targetID)별 텍스트 컴포넌트 저장
    private Dictionary<string, Text> objectiveTexts = new Dictionary<string, Text>();

    private int displayedStepIndex = -1;

    // UI 항목 초기 설정
    public void Setup(Quest quest, PlayerQuestStatus status)
    {
        if (quest == null || status == null) return;

        if (titleText != null)
        {
            titleText.text = quest.questTitle;
            titleText.color = normalColor;
        }

        foreach (Transform child in objectivesContainer)
        {
            Destroy(child.gameObject);
        }

        objectiveTexts.Clear();

        if (status.currentStepIndex < 0 || status.currentStepIndex >= quest.steps.Count)
        {
            displayedStepIndex = -1;
            return;
        }

        QuestStep currentStep = quest.steps[status.currentStepIndex];

        displayedStepIndex = status.currentStepIndex;

        foreach (QuestObjective obj in currentStep.objectives)
        {
            GameObject textGO = Instantiate(objectiveTextPrefab, objectivesContainer);

            Text objectiveText = textGO.GetComponent<Text>();

            if (objectiveText == null) continue;

            int currentAmount = status.objectiveProgress.TryGetValue(obj.targetID, out int amount) ? amount : 0;

            objectiveText.text = FormatObjectiveText(obj, currentAmount);

            objectiveTexts[obj.targetID] = objectiveText;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            GetComponent<RectTransform>());
    }

    // 퀘스트 진행도 업데이트
    public void UpdateProgress(PlayerQuestStatus status, Quest quest)
    {
        if (status == null || quest == null) return;

        // Step이 바뀌었다면 기존 목표 UI를 버리고 새 Step으로 재생성
        if (displayedStepIndex != status.currentStepIndex)
        {
            Setup(quest, status);
            return;
        }

        if (status.currentStepIndex < 0 || status.currentStepIndex >= quest.steps.Count)
        {
            return;
        }

        QuestStep currentStep = quest.steps[status.currentStepIndex];

        foreach (QuestObjective obj in currentStep.objectives)
        {
            if (!objectiveTexts.TryGetValue(obj.targetID, out Text objectiveText))
            {
                continue;
            }

            int currentAmount = status.objectiveProgress.TryGetValue(obj.targetID, out int amount) ? amount: 0;

            objectiveText.text = FormatObjectiveText(obj, currentAmount);
        }
    }

    // 퀘스트 완료 시 시각 효과 적용
    public void SetCompletedVisuals()
    {
        titleText.text = $"[완료] {titleText.text}";
        titleText.color = completedColor;

        // 모든 목표 텍스트를 완료 상태로 변경 (선택 사항)
        foreach (var text in objectiveTexts.Values)
        {
            text.color = completedColor;
            // 예: 텍스트에 취소선 긋기
            // text.fontStyle = FontStyles.Strikethrough;
        }
    }

    // 목표 텍스트 포맷팅
    private string FormatObjectiveText(QuestObjective objective, int currentAmount)
    {
        // 요구사항: "몬스터 이름 (? / ?) 처치하기"
        // TODO: objective.targetID ("MON_Rat")를 실제 이름("쥐")으로 변환하는 시스템 필요
        // 지금은 targetID를 임시로 사용합니다.
        string targetName = GetTargetDisplayName(objective.targetID, objective.type);
        string actionText = GetActionText(objective.type);

        return $"{targetName} ({currentAmount} / {objective.requiredAmount}) {actionText}";
    }

    // (임시) targetID를 표시용 이름으로 변환
    private string GetTargetDisplayName(string targetID, ObjectiveType type)
    {
        // >> 나중에 여기에서 DataManager 같은 것을 참조하여
        // >> "MON_Rat" -> "쥐"
        // >> "ITEM_Herb" -> "약초"
        // >> "NPC_Guard" -> "경비병"
        // >> 등으로 변환해야 합니다.
        return targetID;
    }

    // (임시) 목표 타입에 따른 행동 텍스트 반환
    private string GetActionText(ObjectiveType type)
    {
        switch (type)
        {
            case ObjectiveType.KILL: return "처치하기";
            case ObjectiveType.COLLECT: return "수집하기";
            case ObjectiveType.TALK_TO: return "대화하기";
            default: return "달성하기";
        }
    }
}
