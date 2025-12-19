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
        // (기존과 동일)
        if (PlayerPrefs.HasKey("PlayerName")) txtName.text = PlayerPrefs.GetString("PlayerName");
        else txtName.text = "선생님";

        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int gold = PlayerPrefs.GetInt("PlayerGold", 0);
        txtLevel.text = $"Lv.{level}";
        txtGold.text = string.Format("{0:n0}", gold);
        txtAP.text = "120/120";
    }

    // [1단계] 메인 팝업 열기 (스택 초기화)
    public void OpenPopup(GameObject popup)
    {
        if (popup == null) return;

        popupStack.Clear(); // 스택 깨끗하게 비움
        popupStack.Push(popup); // 첫 번째 패널(예: 일정) 넣기

        popup.SetActive(true);
        if (globalBackButton != null) globalBackButton.SetActive(true);

        // 로비 UI 숨김
        if (characterGroup) characterGroup.SetActive(false);
        if (bottomGroup) bottomGroup.SetActive(false);
        if (rightGroup) rightGroup.SetActive(false);

        Debug.Log($"[OpenPopup] {popup.name} 열림. 스택 수: {popupStack.Count}");
    }

    // [2단계] 깊이 들어가기 (스택 쌓기)
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

        // 1. 현재 패널 끄기
        GameObject current = popupStack.Pop();
        if (current != null) current.SetActive(false);

        Debug.Log($"[Back] {current.name} 닫음. 남은 스택: {popupStack.Count}");

        // 2. 이전 패널이 남아있다면 보여주기
        if (popupStack.Count > 0)
        {
            GameObject prev = popupStack.Peek();
            if (prev != null) prev.SetActive(true);

            // 만약 이전 패널이 'Schedule' 같은 부모라면, 자식들이 꺼졌는지 확인하는 로직은 필요 없음
            // (자식이 꺼지면 부모만 보이게 됨)
        }
        else
        {
            // 3. 스택이 비었으면 로비로
            ReturnToLobby();
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
    public void OnClickBag() => OpenPopup(panelBag);
    public void OnClickStore() => OpenPopup(panelStore);
    public void OnClickSchedule() => OpenPopup(panelSchedule);
}