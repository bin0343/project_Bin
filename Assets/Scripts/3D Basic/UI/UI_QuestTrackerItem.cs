using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_QuestTrackerItem : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public Transform objectivesContainer;
    public GameObject objectiveTextPrefab;
    public Text distanceText;

    [Header("완료 시 시각 효과")]
    public Color completedColor = Color.green;
    public Color normalColor = Color.white;

    // 빠른 접근을 위해 목표(targetID)별 텍스트 컴포넌트 저장
    private Dictionary<string, Text> objectiveTexts = new Dictionary<string, Text>();

    private int displayedStepIndex = -1;

    private string currentDistanceTargetID = "";
    private Transform currentDistanceTarget;

    private void Update()
    {
        UpdateDistance();
    }

    private void UpdateDistance()
    {
        if (distanceText == null) return;

        if (string.IsNullOrEmpty(currentDistanceTargetID))
        {
            distanceText.gameObject.SetActive(false);
            return;
        }

        // 현재 Target을 못 찾고 있다면 다시 검색
        if (currentDistanceTarget == null)
        {
            currentDistanceTarget = QuestTargetMarker.GetTarget(currentDistanceTargetID);
        }

        // 여전히 없다면 거리 표시 안 함
        if (currentDistanceTarget == null || !currentDistanceTarget.gameObject.activeInHierarchy)
        {
            distanceText.gameObject.SetActive(false);
            return;
        }

        Transform player = GetCurrentPlayerTransform();

        if (player == null)
        {
            distanceText.gameObject.SetActive(false);
            return;
        }

        Vector3 playerPos = player.position;
        Vector3 targetPos = currentDistanceTarget.position;

        // 높이 차이는 거리에서 제외
        playerPos.y = 0f;
        targetPos.y = 0f;

        float distance = Vector3.Distance(playerPos, targetPos);

        distanceText.gameObject.SetActive(true);

        distanceText.text = $"{Mathf.FloorToInt(distance)}m";
    }

    private Transform GetCurrentPlayerTransform()
    {
        if (BattleManager.instance != null)
        {
            GameObject activePlayer = BattleManager.instance.GetActiveCharacter();

            if (activePlayer != null)
            {
                return activePlayer.transform;
            }
        }

        return null;
    }

    // UI 항목 초기 설정
    public void Setup(Quest quest, PlayerQuestStatus status)
    {
        if (quest == null || status == null) return;

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

        RefreshDistanceTarget(quest, status);

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
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
        // 모든 목표 텍스트를 완료 상태로 변경 (선택 사항)
        foreach (var text in objectiveTexts.Values)
        {
            text.color = completedColor;
            // 예: 텍스트에 취소선 긋기
            // text.fontStyle = FontStyles.Strikethrough;
        }
    }

    private string FormatObjectiveText(QuestObjective objective, int currentAmount)
    {
        string text = string.IsNullOrWhiteSpace(objective.displayText) ? objective.targetID : objective.displayText;

        if (objective.requiredAmount > 1)
        {
            return $"{text} ({currentAmount} / {objective.requiredAmount})";
        }

        return text;
    }

    private void RefreshDistanceTarget(Quest quest, PlayerQuestStatus status)
    {
        currentDistanceTargetID = "";
        currentDistanceTarget = null;

        if (distanceText != null)
        {
            distanceText.gameObject.SetActive(false);
        }

        if (quest == null || status == null) return;

        if (status.currentStepIndex < 0 || status.currentStepIndex >= quest.steps.Count)
        {
            return;
        }

        QuestStep currentStep = quest.steps[status.currentStepIndex];

        if (currentStep.objectives == null || currentStep.objectives.Count == 0)
        {
            return;
        }

        // QuestManager와 동일하게 현재 Step의 첫 번째 Objective를 대표 목표로 사용
        QuestObjective objective = currentStep.objectives[0];

        currentDistanceTargetID = objective.targetID;

        currentDistanceTarget = QuestTargetMarker.GetTarget(currentDistanceTargetID);
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
            case ObjectiveType.LOCATION: return "이동하기";
            default: return "달성하기";
        }
    }
}
