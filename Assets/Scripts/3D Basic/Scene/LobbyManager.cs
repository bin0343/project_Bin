using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("--- 텍스트 UI 연결 (Group_Top) ---")]
    public Text txtName;
    public Text txtLevel;
    public Text txtGold;
    public Text txtAP;

    [Header("--- 로비 메인 요소 (팝업 뜰 때 숨길 것들) ---")]
    public GameObject characterGroup; // Hierarchy의 'Character' 오브젝트
    public GameObject topGroup;       // Hierarchy의 'Group_Top' (선택: 팝업 때 켜둘지 끌지)
    public GameObject bottomGroup;    // Hierarchy의 'Group_Bottom'
    public GameObject rightGroup;     // Hierarchy의 'Group_Right'

    [Header("--- 팝업 패널 (새로 만들어야 함) ---")]
    public GameObject panelClub;      // 동아리 패널
    public GameObject panelBag;       // 가방 패널
    public GameObject panelStore;     // 상점 패널
    public GameObject panelSchedule;  // 일정 패널

    // 현재 열려있는 팝업 기억용
    private GameObject currentPopup = null;

    void Start()
    {
        // 1. 유저 정보 표시
        RefreshUserInfo();

        // 2. 시작할 때 모든 팝업 끄기
        CloseAllPopups();
    }

    void RefreshUserInfo()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
            txtName.text = PlayerPrefs.GetString("PlayerName");
        else
            txtName.text = "선생님";

        // 임시 데이터 (나중엔 GameManager 연동)
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int gold = PlayerPrefs.GetInt("PlayerGold", 0);

        txtLevel.text = $"Lv.{level}";
        txtGold.text = string.Format("{0:n0}", gold);
        txtAP.text = "120/120";
    }

    // ====================================================
    // 공통 기능: 팝업 열기/닫기 로직
    // ====================================================

    void OpenPopup(GameObject popup)
    {
        if (popup == null) return;

        currentPopup = popup;

        // 1. 팝업 켜기
        popup.SetActive(true);

        // 2. 로비 UI 숨기기 (캐릭터와 하단 메뉴 등)
        if (characterGroup) characterGroup.SetActive(false);
        if (bottomGroup) bottomGroup.SetActive(false);
        if (rightGroup) rightGroup.SetActive(false);

        // (선택) 상단바도 가리고 싶으면 아래 주석 해제
        // if(topGroup) topGroup.SetActive(false);
    }

    // 뒤로가기 버튼에 연결할 함수
    public void OnClickBack()
    {
        // 팝업 닫기
        if (currentPopup != null)
        {
            currentPopup.SetActive(false);
            currentPopup = null;
        }

        // 로비 UI 다시 보이기
        if (characterGroup) characterGroup.SetActive(true);
        if (bottomGroup) bottomGroup.SetActive(true);
        if (rightGroup) rightGroup.SetActive(true);
        if (topGroup) topGroup.SetActive(true);
    }

    void CloseAllPopups()
    {
        if (panelClub) panelClub.SetActive(false);
        if (panelBag) panelBag.SetActive(false);
        if (panelStore) panelStore.SetActive(false);
        if (panelSchedule) panelSchedule.SetActive(false);
    }

    // ====================================================
    // 버튼 연결 함수
    // ====================================================

    public void OnClickBattle()
    {
        // 전투는 씬 이동
        SceneManager.LoadScene("Village");
    }

    public void OnClickClub()
    {
        OpenPopup(panelClub);
    }

    public void OnClickBag()
    {
        OpenPopup(panelBag);
    }

    public void OnClickStore()
    {
        OpenPopup(panelStore);
    }

    public void OnClickSchedule()
    {
        OpenPopup(panelSchedule);
    }
}