using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [Header("시간 설정")]
    public int year = 2026;
    public int month = 1;
    public int day = 1;
    public System.DayOfWeek currentDayOfWeek;
    public TimeOfDay currentTimeOfDay = TimeOfDay.Morning;

    public event Action<int, int, int> OnDayChanged; // 날짜가 바뀌면 알림 (년, 월, 일)
    public event Action<TimeOfDay> OnTimeChanged;    // 시간대가 바뀌면 알림

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작 시 한 번 갱신
        UpdateDayOfWeek();
    }

    // --- [기능 1] 시간대 진행 (행동력 소모 후 호출) ---
    public void AdvanceTime()
    {
        // 다음 시간대로 넘어감
        currentTimeOfDay++;

        // 밤(Night)을 지나면 -> 다음 날 아침으로
        if (currentTimeOfDay > TimeOfDay.Night)
        {
            NextDay();
        }
        else
        {
            // 시간대 변경 알림
            OnTimeChanged?.Invoke(currentTimeOfDay);
            Debug.Log($"시간 변경: {currentTimeOfDay}");
        }
        UpdateUI();
    }

    // --- [기능 2] 하루 넘기기 (취침 시 호출) ---
    public void NextDay()
    {
        currentTimeOfDay = TimeOfDay.Morning;
        day++;

        // [수정] 이번 달이 며칠까지 있는지 확인 (28, 30, 31, 윤년 자동 계산)
        int daysInThisMonth = DateTime.DaysInMonth(year, month);

        if (day > daysInThisMonth)
        {
            day = 1;
            month++;
            if (month > 12)
            {
                month = 1;
                year++;
            }
        }

        UpdateDayOfWeek(); // 날짜가 바뀌었으니 요일 갱신

        Debug.Log($"[날짜 변경] {year}년 {month}월 {day}일 ({currentDayOfWeek})");
        OnDayChanged?.Invoke(year, month, day);
        OnTimeChanged?.Invoke(currentTimeOfDay);

        // UI 갱신 (CalendarController가 있다면)
        if (CalendarController._calendarInstance != null &&
            CalendarController._calendarInstance._calendarPanel.activeSelf)
        {
            CalendarController._calendarInstance.CreateCalendar(); // 열려있으면 갱신
        }
    }

    void UpdateDayOfWeek()
    {
        // DateTime 생성자는 (년, 월, 일)을 받습니다.
        try
        {
            DateTime date = new DateTime(year, month, day);
            currentDayOfWeek = date.DayOfWeek; // System.DayOfWeek 사용
        }
        catch (Exception e)
        {
            Debug.LogError("날짜 오류: " + e.Message);
        }
    }

    // 행동력 회복 로직
    void RecoverAP()
    {
        if (Player_Stat.globalInstance != null)
        {
            // 예: 최대 AP로 회복 (Player_Stat에 maxAP 변수 필요)
            // Player_Stat.globalInstance.currentAP = Player_Stat.globalInstance.maxAP;
            Debug.Log("새로운 하루가 시작되어 행동력이 회복되었습니다.");
        }
    }

    // UI 갱신 요청
    void UpdateUI()
    {
        if (UI_Calendar.instance != null)
        {
            UI_Calendar.instance.UpdateCalendarUI();
        }
    }
}