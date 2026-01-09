using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum QuestTab { InProgress, Completed }

public class UI_QuestPanel : MonoBehaviour
{
    [Header("--- 왼쪽: 리스트 영역 ---")]
    public Transform contentParent; // ScrollView의 Content
    public GameObject questSlotPrefab; // UI_QuestSlot 프리팹

    [Header("--- 탭 설정 ---")]
    public Toggle toggleInProgress;
    public Toggle toggleCompleted;
    // (하이라이트용) 탭의 배경이나 텍스트를 연결하세요
    public Image imgInProgressBg;
    public Image imgCompletedBg;
    public Text txtInProgress;
    public Text txtCompleted;

    public Color activeColor = Color.white;    // 선택된 탭 색상 (예: 밝은색)
    public Color inactiveColor = Color.gray;   // 선택 안 된 탭 색상 (예: 어두운색)

    [Header("--- 오른쪽: 상세 정보 영역 ---")]
    public GameObject detailsGroup;
    public Text txtTitle;
    public Text txtDescription;
    public Text txtObjective;
    public Text txtReward;
    public Text txtTimeLimit;

    private QuestTab currentTab = QuestTab.InProgress;
    private List<UI_QuestSlot> createdSlots = new List<UI_QuestSlot>();

    private void OnEnable()
    {
        // 1. 켜질 때 강제로 '진행 중' 상태로 초기화
        currentTab = QuestTab.InProgress;

        // 토글 상태 강제 설정 (이벤트 발생 방지를 위해 리스너 잠깐 끄거나, 그냥 호출 후 UI 갱신)
        if (toggleInProgress != null)
        {
            toggleInProgress.SetIsOnWithoutNotify(true);
        }
        if (toggleCompleted != null)
        {
            toggleCompleted.SetIsOnWithoutNotify(false);
        }

        // 2. UI 및 리스트 즉시 갱신
        UpdateTabVisuals();
        RefreshList();
        ClearDetails();
    }

    // --- 탭 버튼(Toggle) 이벤트 연결 ---
    public void OnTabChanged(bool isOn)
    {
        // 토글은 켜질 때(true)와 꺼질 때(false) 두 번 호출되므로, 켜진 놈 기준일 때만 로직 실행
        if (toggleInProgress.isOn)
        {
            currentTab = QuestTab.InProgress;
        }
        else
        {
            currentTab = QuestTab.Completed;
        }

        UpdateTabVisuals();
        RefreshList();
        ClearDetails();
    }

    // 탭 색상 변경 로직 (하이라이트)
    void UpdateTabVisuals()
    {
        bool isInProgress = (currentTab == QuestTab.InProgress);

        // 진행중 탭 색상
        if (imgInProgressBg) imgInProgressBg.color = isInProgress ? activeColor : inactiveColor;
        if (txtInProgress) txtInProgress.color = isInProgress ? activeColor : inactiveColor;

        // 완료 탭 색상
        if (imgCompletedBg) imgCompletedBg.color = !isInProgress ? activeColor : inactiveColor;
        if (txtCompleted) txtCompleted.color = !isInProgress ? activeColor : inactiveColor;
    }

    // --- 리스트 갱신 ---
    void RefreshList()
    {
        // 1. 기존 슬롯 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        createdSlots.Clear();

        if (QuestManager.instance == null) return;

        // 2. 퀘스트 로드 및 필터링
        foreach (var pair in QuestManager.instance.questLog)
        {
            string qID = pair.Key;
            PlayerQuestStatus status = pair.Value;
            Quest questData = QuestManager.instance.GetQuestByID(qID);

            if (questData == null) continue;

            bool showThis = false;

            if (currentTab == QuestTab.InProgress)
            {
                // [수정] 진행 중이거나, 목표는 달성했지만 아직 보상을 안 받은 경우(COMPLETED)까지 보여줌
                if (status.status == QuestStatus.IN_PROGRESS || status.status == QuestStatus.COMPLETED)
                    showThis = true;
            }
            else // Completed Tab
            {
                // [수정] 보상을 완전히 받은 경우(REWARD_CLAIMED)나 실패한 경우만 보여줌
                if (status.status == QuestStatus.REWARD_CLAIMED || status.status == QuestStatus.FAILED)
                    showThis = true;
            }

            if (showThis)
            {
                GameObject obj = Instantiate(questSlotPrefab, contentParent);
                UI_QuestSlot slot = obj.GetComponent<UI_QuestSlot>();

                string displayTitle = questData.questTitle;

                // 상태별 꼬리표
                if (status.status == QuestStatus.FAILED) displayTitle += " (실패)";
                else if (status.status == QuestStatus.COMPLETED) displayTitle += " (완료 가능!)";
                else if (status.status == QuestStatus.REWARD_CLAIMED) displayTitle += " (완료됨)";

                slot.Setup(qID, displayTitle, OnSlotClicked);
                createdSlots.Add(slot);
            }
        }
    }

    // --- 슬롯 클릭 시 상세 정보 ---
    void OnSlotClicked(string questID)
    {
        foreach (var slot in createdSlots) slot.Deselect();

        Quest questData = QuestManager.instance.GetQuestByID(questID);
        PlayerQuestStatus userStatus = QuestManager.instance.questLog[questID];
        if (questData == null) return;

        detailsGroup.SetActive(true);

        txtTitle.text = questData.questTitle;
        txtDescription.text = questData.description;

        // 목표 표시
        string objectiveStr = "";
        foreach (var obj in questData.objectives)
        {
            int current = userStatus.objectiveProgress.ContainsKey(obj.targetID) ? userStatus.objectiveProgress[obj.targetID] : 0;
            // 목표 달성 시 색상 변경 등 가능
            string colorHex = (current >= obj.requiredAmount) ? "green" : "black";
            objectiveStr += $"<color={colorHex}>- {obj.targetID} : {current} / {obj.requiredAmount}</color>\n";
        }
        txtObjective.text = objectiveStr;

        // 보상 표시
        txtReward.text = $"골드: {questData.rewards.gold} G\n경험치: {questData.rewards.experience} Exp";

        // 기간 표시
        if (questData.hasTimeLimit)
        {
            txtTimeLimit.text = $"마감: {questData.dueYear}년 {questData.dueMonth}월 {questData.dueDay}일까지";
            txtTimeLimit.color = Color.red;
        }
        else
        {
            txtTimeLimit.text = "기간 제한 없음";
            txtTimeLimit.color = Color.black;
        }
    }

    void ClearDetails()
    {
        if (detailsGroup) detailsGroup.SetActive(false);
    }
}