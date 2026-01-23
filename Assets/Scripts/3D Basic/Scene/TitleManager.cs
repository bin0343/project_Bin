using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("--- UI 버튼 ---")]
    public Button btnNewGame;
    public Button btnContinue;

    [Header("--- 씬 설정 ---")]
    public string lobbySceneName = "Main Menu";

    void Start()
    {
        // 저장된 데이터(이름)가 있는지 확인
        bool hasSaveData = PlayerPrefs.HasKey("PlayerName");

        // 이어하기 버튼 활성화/비활성화
        btnContinue.interactable = hasSaveData;

        // 버튼 리스너 연결
        btnNewGame.onClick.AddListener(OnClickNewGame);
        btnContinue.onClick.AddListener(OnClickContinue);
    }

    void OnClickNewGame()
    {
        // 기존 데이터 삭제
        PlayerPrefs.DeleteAll();

        // [핵심] "이 사람은 신입생이다"라는 표식 남기기
        PlayerPrefs.SetInt("IsFirstVisit", 1);

        // 기본 재화 설정 (이름은 아직 없음!)
        PlayerPrefs.SetInt("PlayerLevel", 1);
        PlayerPrefs.SetInt("PlayerGold", 0);

        PlayerPrefs.Save();

        SceneManager.LoadScene(lobbySceneName);
    }

    void OnClickContinue()
    {
        SceneManager.LoadScene(lobbySceneName);
    }
}