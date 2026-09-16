using UnityEngine;
using UnityEngine.UI;
using System; // System.DayOfWeek 사용을 위해 추가

public class UI_Calendar : MonoBehaviour
{
    public static UI_Calendar instance;

    [Header("UI 연결")]
    public Text dateText;   // 예: "1월 15일"
    public Text dayText;    // 예: "수"
    public Text timeText;   // 예: "오후"
    public Image timeIcon;  // 아침/점심/저녁 아이콘

    [Header("시간대별 아이콘 (옵션)")]
    public Sprite iconMorning;
    public Sprite iconAfternoon;
    public Sprite iconEvening;
    public Sprite iconNight;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UpdateCalendarUI();
    }

    public void UpdateCalendarUI()
    {
        if (TimeManager.instance == null) return;

        // 1. 텍스트 갱신
        // TimeManager.instance.currentDayOfWeek는 이제 System.DayOfWeek 타입입니다.
        string dayStr = GetDayString(TimeManager.instance.currentDayOfWeek);

        if (dateText != null)
        {
            dateText.text = $"{TimeManager.instance.month} / {TimeManager.instance.day}";
        }

        if (dayText != null)
        {
            dayText.text = $"{dayStr}";
        }

        // 2. 시간 텍스트/아이콘 갱신
        TimeOfDay time = TimeManager.instance.currentTimeOfDay;
        if (timeText != null)
        {
            timeText.text = TranslateTime(time);
        }

        if (timeIcon != null)
        {
            switch (time)
            {
                case TimeOfDay.Morning: timeIcon.sprite = iconMorning; break;
                case TimeOfDay.Afternoon: timeIcon.sprite = iconAfternoon; break;
                case TimeOfDay.Evening: timeIcon.sprite = iconEvening; break;
                case TimeOfDay.Night: timeIcon.sprite = iconNight; break;
            }
        }
    }

    // [수정] 매개변수 타입을 System.DayOfWeek로 변경
    string GetDayString(System.DayOfWeek day)
    {
        switch (day)
        {
            // System.DayOfWeek의 요일 이름 사용 (Monday, Tuesday...)
            case System.DayOfWeek.Monday: return "월";
            case System.DayOfWeek.Tuesday: return "화";
            case System.DayOfWeek.Wednesday: return "수";
            case System.DayOfWeek.Thursday: return "목";
            case System.DayOfWeek.Friday: return "금";
            case System.DayOfWeek.Saturday: return "토";
            case System.DayOfWeek.Sunday: return "일";
            default: return "";
        }
    }

    string TranslateTime(TimeOfDay time)
    {
        switch (time)
        {
            case TimeOfDay.Morning: return "아침";
            case TimeOfDay.Afternoon: return "오후";
            case TimeOfDay.Evening: return "저녁";
            case TimeOfDay.Night: return "밤";
            default: return "";
        }
    }
}