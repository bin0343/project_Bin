using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class TitleManager : MonoBehaviour
{
    [Header("--- UI 버튼 ---")]
    public Button btnNewGame;
    public Button btnContinue;

    [Header("--- 씬 설정 ---")]
    public string lobbySceneName = "Main Menu";

    void Start()
    {
        // 1. JSON 저장 파일이 실제로 존재하는지 확인
        // (GameDataManager가 초기화된 후 경로를 가져옴)
        string path = Path.Combine(Application.persistentDataPath, "MyGameSave.json");
        bool hasSaveData = File.Exists(path);

        // 2. 이어하기 버튼 활성화/비활성화
        btnContinue.interactable = hasSaveData;

        btnNewGame.onClick.AddListener(OnClickNewGame);
        btnContinue.onClick.AddListener(OnClickContinue);
    }

    void OnClickNewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("IsFirstVisit", 1); // 튜토리얼 다시 보게 설정

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.saveData = new SaveData(); // 텅 빈 새 데이터

            // 기본 지급 아이템이나 초기 골드가 필요하면 여기서 설정
            // 예: GameDataManager.Instance.saveData.playerGold = 1000;

            GameDataManager.Instance.SaveGame(); // 초기 상태 덮어쓰기 저장
        }

        SceneManager.LoadScene(lobbySceneName);
    }

    void OnClickContinue()
    {
        if (GameDataManager.Instance != null)
        {
            bool success = GameDataManager.Instance.LoadGame();
            if (success)
            {
                SceneManager.LoadScene(lobbySceneName);
            }
            else
            {
                Debug.LogError("저장된 파일을 불러오는데 실패했습니다.");
            }
        }
    }
}