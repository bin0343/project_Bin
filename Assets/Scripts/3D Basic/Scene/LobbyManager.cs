using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [Header("--- 텍스트 UI 연결 ---")]
    public Text txtName;
    public Text txtLevel;
    public Text txtGold;
    public Text txtAP;

    [Header("--- 로비 UI 그룹 ---")]
    public GameObject characterGroup;
    public GameObject topGroup;
    public GameObject bottomGroup;
    public GameObject rightGroup;

    [Header("--- 팝업 패널 ---")]
    public GameObject panelClub;
    public GameObject panelBag;
    public GameObject panelStore;
    public GameObject panelSchedule;
    public GameObject panelCalendar;
    public GameObject panelQuest;

    [Header("페이드 설정")]
    public Image screenFader;
    public float fadeDuration = 0.5f;

    [Header("--- 전역 뒤로가기 버튼 ---")]
    public GameObject globalBackButton;

    [Header("튜토리얼 전용 버튼 제어")]
    public GameObject btnSchedule;
    public GameObject[] otherButtons;

    // 스택 선언
    private Stack<GameObject> popupStack = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        RefreshUserInfo();
        CloseAllPopups();
        if (globalBackButton != null) globalBackButton.SetActive(false);
        screenFader.gameObject.SetActive(false);
    }

    //튜토리얼 전용
    public void ShowOnlyScheduleButton()    //일정 버튼만 활성화
    {
        SetLobbyUIVisible(false);

        if (bottomGroup) bottomGroup.SetActive(true);

        if (btnSchedule) btnSchedule.SetActive(true);

        if (otherButtons != null)
        {
            foreach (var btn in otherButtons)
            {
                if (btn != null) btn.SetActive(false);
            }
        }
    }

    public void RestoreAllBottomButtons()
    {
        if (btnSchedule) btnSchedule.SetActive(true);

        if (otherButtons != null)
        {
            foreach (var btn in otherButtons)
            {
                if (btn != null) btn.SetActive(true);
            }
        }

        if (bottomGroup) bottomGroup.SetActive(true);
    }

    public void RefreshUserInfo()
    {
        if (txtName != null) txtName.text = PlayerPrefs.GetString("PlayerName", "학생");

        //Character_Stat playerStat = Character_Stat;

        /*if (playerStat == null && Player_Inventory.Instance != null)
        {
            playerStat = Player_Inventory.Instance.GetComponent<Player_Stat>();
        }

        if (playerStat != null)
        {
            if (txtGold != null) txtGold.text = string.Format("{0:n0}G", playerStat.gold);
            if (txtLevel != null) txtLevel.text = "Lv." + playerStat.level;

            Debug.Log($"로비 UI 갱신: {playerStat.gold}G");
        }
        else
        {
            // 데이터가 없을 때 (테스트용)
            if (txtGold != null) txtGold.text = PlayerPrefs.GetInt("PlayerGold", 0).ToString();
        }*/
    }

    public void OpenPopup(GameObject popup)
    {
        if (popup == null || screenFader == null) return;

        StartFadeEffect(() => {
            ExecuteOpenPopup(popup);
        }, 1.0f);
    }

    private void ExecuteOpenPopup(GameObject popup)
    {
        popupStack.Clear();
        popupStack.Push(popup);

        popup.SetActive(true);
        if (globalBackButton != null) globalBackButton.SetActive(true);

        if (characterGroup) characterGroup.SetActive(false);
        if (bottomGroup) bottomGroup.SetActive(false);
        if (rightGroup) rightGroup.SetActive(false);
    }

    public void OpenDepthPanel(GameObject nextPanel)
    {
        if (nextPanel == null) return;

        // 중복 방지: 이미 스택 맨 위에 있는 패널을 또 열려고 하면 무시
        if (popupStack.Count > 0 && popupStack.Peek() == nextPanel)
        {
            return;
        }

        popupStack.Push(nextPanel);
        nextPanel.SetActive(true);

        Debug.Log($"[Depth] {nextPanel.name} 진입. 스택 수: {popupStack.Count}");
    }

    // [뒤로가기]
    public void OnClickBack()
    {
        if (popupStack.Count == 0)
        {
            ReturnToLobby();
            return;
        }

        GameObject current = popupStack.Pop();
        if (current != null)
        {
            // 뒤로가기는 대기 시간 없이 바로 전환되도록
            StartFadeEffect(() => {
                current.SetActive(false);
                if (popupStack.Count > 0)
                {
                    GameObject prev = popupStack.Peek();
                    if (prev != null) prev.SetActive(true);
                }
                else
                {
                    ReturnToLobby();
                }
            }, 0f); // 대기 시간 0초 전달
        }
    }

    public void OnClickBag()
    {
        OpenPopup(panelBag);

        // 가방을 열 때 UI를 새로고침
        UI_Inventory uiInv = panelBag.GetComponent<UI_Inventory>();
        if (uiInv != null)
        {
            uiInv.ChangeTab(UI_Inventory.InventoryTabType.ALL); // 기본 탭으로 열기
            uiInv.RefreshUI();
        }
    }

    public void OnClickQuestButton()
    {
        if (panelQuest != null)
        {
            OpenPopup(panelQuest);
        }
    }

    public void OnClickCalendarButton()
    {
        if (panelCalendar != null)
        {
            panelCalendar.SetActive(true);
        }
        else
        {
            Debug.LogError("LobbyManager에 Panel_Calendar가 연결되지 않았습니다!");
        }
    }

    public void OnClickStore()
    {
        if (panelStore != null)
        {
            OpenPopup(panelStore);
        }
        else
        {
            Debug.LogError("LobbyManager에 panel_Store가 연결되지 않았습니다!");
        }
    }

    void ReturnToLobby()
    {
        Debug.Log("[Lobby] 로비로 복귀");
        if (globalBackButton != null) globalBackButton.SetActive(false);

        if (characterGroup) characterGroup.SetActive(true);
        if (bottomGroup) bottomGroup.SetActive(true);
        if (rightGroup) rightGroup.SetActive(true);
        if (topGroup) topGroup.SetActive(true);

        CloseAllPopups(); // 안전하게 모든 팝업 끄기

        RefreshUserInfo();
    }

    void CloseAllPopups()
    {
        if (panelClub) panelClub.SetActive(false);
        if (panelBag) panelBag.SetActive(false);
        if (panelStore) panelStore.SetActive(false);
        if (panelSchedule) panelSchedule.SetActive(false);
        if (panelCalendar) panelCalendar.SetActive(false);
        if (panelQuest) panelQuest.SetActive(false);
        popupStack.Clear();
    }

    public void SetLobbyUIVisible(bool isVisible)
    {
        if (characterGroup) characterGroup.SetActive(isVisible);
        if (topGroup) topGroup.SetActive(isVisible);
        if (bottomGroup) bottomGroup.SetActive(isVisible);
        if (rightGroup) rightGroup.SetActive(isVisible);
        if (globalBackButton) globalBackButton.SetActive(false);
    }

    // 버튼 연결용 함수들
    public void OnClickBattle() => SceneManager.LoadScene("Battle");
    public void OnClickClub() => OpenPopup(panelClub);
    
    
    public void OnClickSchedule() => OpenPopup(panelSchedule);

    public void StartFadeEffect(System.Action onMidWay, float waitTime)
    {
        if (screenFader == null) return;

        screenFader.DOKill();
        Sequence fadeSeq = DOTween.Sequence().SetUpdate(true);

        screenFader.gameObject.SetActive(true);

        // 화면 검게 만들기 (Fade In)
        fadeSeq.Append(screenFader.DOFade(1f, fadeDuration));

        // 전달받은 waitTime만큼 대기 (뒤로가기는 0, 일반 오픈은 1.0)
        if (waitTime > 0)
        {
            fadeSeq.AppendInterval(waitTime);
        }

        // 화면이 검은 상태에서 로직 실행
        fadeSeq.AppendCallback(() => {
            onMidWay?.Invoke();
        });

        // 화면 다시 밝게 만들기 (Fade Out)
        fadeSeq.Append(screenFader.DOFade(0f, fadeDuration));

        fadeSeq.OnComplete(() => {
            screenFader.gameObject.SetActive(false);
        });
    }
}