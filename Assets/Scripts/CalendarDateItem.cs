using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CalendarDateItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 연결")]
    public Text dayText;      // 날짜 숫자 텍스트
    public Image bgImage;     // 배경 이미지
    public GameObject eventDot; // 이벤트 표시 점

    [Header("색상 설정")]
    public Color normalColor = Color.white;      // 평일
    public Color todayColor = new Color(1f, 0.8f, 0.8f); // 오늘
    public Color emptyColor = new Color(0, 0, 0, 0); // 투명

    private int _year, _month, _day;

    // [핵심] CalendarController가 이 함수를 부릅니다.
    public void Setup(int year, int month, int day, bool isToday, bool isEmpty)
    {
        _year = year;
        _month = month;
        _day = day;

        // 1. 빈 칸인 경우
        if (isEmpty)
        {
            if (dayText) dayText.text = "";
            if (bgImage) bgImage.color = emptyColor;
            if (eventDot) eventDot.SetActive(false);
            return;
        }

        // 2. 날짜가 있는 경우
        if (dayText) dayText.text = day.ToString();

        // 오늘이면 색깔 변경
        if (bgImage) bgImage.color = isToday ? todayColor : normalColor;

        if (eventDot) eventDot.SetActive(false);
    }

    // 마우스 올렸을 때 (나중에 툴팁 기능 구현 가능)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (dayText.text == "") return;
        // Debug.Log($"{_year}-{_month}-{_day}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}