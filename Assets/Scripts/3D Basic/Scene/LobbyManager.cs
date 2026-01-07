using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

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

    [Header("--- 전역 뒤로가기 버튼 ---")]
    public GameObject globalBackButton;

    // 스택 선언
    private Stack<GameObject> popupStack = new Stack<GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        RefreshUserInfo();
        CloseAllPopups();
        if (globalBackButton != null) globalBackButton.SetActive(false);
    }

    void RefreshUserInfo()
    {
        if (txtName != null) txtName.text = PlayerPrefs.GetString("PlayerName", "학생");

        // Global Manager에서 Player_Stat 찾기
        Player_Stat playerStat = Player_Stat.globalInstance;

        // 만약 globalInstance가 설정 안 되어 있다면 수동으로 찾기
        if (playerStat == null && Player_Inventory.instance != null)
        {
            playerStat = Player_Inventory.instance.GetComponent<Player_Stat>();
        }

        if (playerStat != null)
        {
            if (txtGold != null) txtGold.text = string.Format("{0:n0}G", playerStat.gold);
            if (txtLevel != null) txtLevel.text = "Lv." + playerStat.level;

            // 데이터가 잘 연결되었는지 로그 확인
            // Debug.Log($"로비 UI 갱신: {playerStat.gold}G");
        }
        else
        {
            // 데이터가 없을 때 (테스트용)
            if (txtGold != null) txtGold.text = PlayerPrefs.GetInt("PlayerGold", 0).ToString();
        }
    }

    public void OpenPopup(GameObject popup)
    {
        if (popup == null) return;

        popupStack.Clear(); 
        popupStack.Push(popup);

        popup.SetActive(true);
        if (globalBackButton != null) globalBackButton.SetActive(true);

        // 로비 UI 숨김
        if (characterGroup) characterGroup.SetActive(false);
        if (bottomGroup) bottomGroup.SetActive(false);
        if (rightGroup) rightGroup.SetActive(false);

        Debug.Log($"[OpenPopup] {popup.name} 열림. 스택 수: {popupStack.Count}");
    }

    public void OpenDepthPanel(GameObject nextPanel)
    {
        if (nextPanel == null) return;

        // 중복 방지: 이미 스택 맨 위에 있는 패널을 또 열려고 하면 무시
        if (popupStack.Count > 0 && popupStack.Peek() == nextPanel)
        {
            return;
        }

        // 이전 패널 끄지 않음 (부모-자식 관계 유지 위해)
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
        if (current != null) current.SetActive(false);

        Debug.Log($"[Back] {current.name} 닫음. 남은 스택: {popupStack.Count}");

        if (popupStack.Count > 0)
        {
            GameObject prev = popupStack.Peek();
            if (prev != null) prev.SetActive(true);
        }
        else
        {
            ReturnToLobby();
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
        popupStack.Clear();
    }

    // 버튼 연결용 함수들
    public void OnClickBattle() => SceneManager.LoadScene("Battle");
    public void OnClickClub() => OpenPopup(panelClub);
    
    public void OnClickStore() => OpenPopup(panelStore);
    public void OnClickSchedule() => OpenPopup(panelSchedule);
}