using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UI_QuestSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 연결")]
    public Text titleText;
    public Image statusIcon; // (선택) 진행중/완료 아이콘
    public GameObject selectHighlight; // 선택됐을 때 강조 효과

    private string myQuestID;
    private Action<string> onClickCallback; // 클릭 시 실행할 함수 저장

    // 데이터 세팅 함수
    public void Setup(string questID, string title, Action<string> onClick)
    {
        myQuestID = questID;
        if (titleText != null) titleText.text = title;
        onClickCallback = onClick;

        if (selectHighlight != null) selectHighlight.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭되면 UI_QuestPanel에 내 ID를 전달
        onClickCallback?.Invoke(myQuestID);

        // (선택 효과)
        if (selectHighlight != null) selectHighlight.SetActive(true);
        if (titleText != null) titleText.color = Color.black;
    }

    // 다른 놈이 선택됐을 때 내 강조 끄기용
    public void Deselect()
    {
        if (selectHighlight != null) selectHighlight.SetActive(false);
        if (titleText != null) titleText.color = Color.white;
    }
}