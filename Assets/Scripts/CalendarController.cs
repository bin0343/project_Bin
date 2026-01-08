using System; // DateTime
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalendarController : MonoBehaviour
{
    public static CalendarController _calendarInstance;

    [Header("UI 연결")]
    public GameObject _calendarPanel;
    public Text _yearNumText;
    public Text _monthNumText;

    [Header("그리드 설정")]
    public Transform _dateGridParent;
    public GameObject _itemPrefab;

    // 현재 보고 있는 달력의 연/월
    private int displayYear;
    private int displayMonth;

    void Awake()
    {
        _calendarInstance = this;
    }

    private void OnEnable()
    {
        // OpenCalendar 로직을 여기서 실행
        if (TimeManager.instance != null)
        {
            // 현재 시간으로 설정
            displayYear = TimeManager.instance.year;
            displayMonth = TimeManager.instance.month;
        }
        else
        {
            displayYear = 2026;
            displayMonth = 1;
        }

        CreateCalendar(); // 달력 그리기
    }

    public void OpenCalendar()
    {
        _calendarPanel.SetActive(true);

        if (TimeManager.instance != null)
        {
            displayYear = TimeManager.instance.year;
            displayMonth = TimeManager.instance.month;
        }
        else
        {
            displayYear = 2024;
            displayMonth = 1;
        }

        CreateCalendar();
    }

    public void CloseCalendar()
    {
        _calendarPanel.SetActive(false);
    }

    // [핵심 수정] DateTime을 이용한 달력 생성
    public void CreateCalendar()
    {
        // 1. 초기화
        foreach (Transform child in _dateGridParent) Destroy(child.gameObject);

        // 2. 텍스트 갱신
        if (_yearNumText) _yearNumText.text = displayYear + "년";
        if (_monthNumText) _monthNumText.text = displayMonth + "월";

        // 3. 계산: 이번 달 1일은 무슨 요일인가?
        DateTime firstDayOfMonth = new DateTime(displayYear, displayMonth, 1);
        int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek; // 일요일(0) ~ 토요일(6)

        // 4. 계산: 이번 달은 며칠까지 있는가?
        int daysInMonth = DateTime.DaysInMonth(displayYear, displayMonth);

        // 5. 앞쪽 빈칸 채우기 (일요일부터 시작하는 달력 기준)
        for (int i = 0; i < startDayOfWeek; i++)
        {
            SpawnSlot(0, true);
        }

        // 6. 날짜 채우기
        int currentYear = TimeManager.instance != null ? TimeManager.instance.year : 0;
        int currentMonth = TimeManager.instance != null ? TimeManager.instance.month : 0;
        int currentDay = TimeManager.instance != null ? TimeManager.instance.day : 0;

        for (int d = 1; d <= daysInMonth; d++)
        {
            bool isToday = (displayYear == currentYear && displayMonth == currentMonth && d == currentDay);
            SpawnSlot(d, false, isToday);
        }
    }

    void SpawnSlot(int day, bool isEmpty, bool isToday = false)
    {
        GameObject itemObj = Instantiate(_itemPrefab, _dateGridParent);
        CalendarDateItem itemScript = itemObj.GetComponent<CalendarDateItem>();

        if (itemScript != null)
        {
            itemScript.Setup(displayYear, displayMonth, day, isToday, isEmpty);
        }
    }

    // 월 이동 버튼
    public void MonthPrev()
    {
        displayMonth--;
        if (displayMonth < 1)
        {
            displayMonth = 12;
            displayYear--;
        }
        CreateCalendar();
    }

    public void MonthNext()
    {
        displayMonth++;
        if (displayMonth > 12)
        {
            displayMonth = 1;
            displayYear++;
        }
        CreateCalendar();
    }
}