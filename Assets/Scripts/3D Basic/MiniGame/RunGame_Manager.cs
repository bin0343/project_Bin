using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RunGame_Manager : MonoBehaviour
{
    public static RunGame_Manager instance;

    [Header("UI 연결")]
    public Text timerText;
    public GameObject resultPanel;
    public Text rankText;
    public Text rewardText;

    [Header("설정")]
    public bool isGameOver = false;
    private float survivalTime = 0f;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (!isGameOver)
        {
            survivalTime += Time.deltaTime;
            timerText.text = $"Time: {survivalTime:F2}s";
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // 결과 계산
        string rank = "C";
        int goldReward = 0;
        int expReward = 0;

        if (survivalTime >= 60f) { rank = "S"; goldReward = 500; expReward = 100; }
        else if (survivalTime >= 45f) { rank = "A"; goldReward = 300; expReward = 70; }
        else if (survivalTime >= 30f) { rank = "B"; goldReward = 100; expReward = 40; }
        else { rank = "C"; goldReward = 10; expReward = 10; }

        // 보상 지급 (PlayerPrefs에 저장 -> 로비 매니저가 읽어서 반영)
        // 1. 골드
        int currentGold = PlayerPrefs.GetInt("PlayerGold", 0);
        PlayerPrefs.SetInt("PlayerGold", currentGold + goldReward);

        // 2. 경험치 (플레이어 레벨 데이터가 있다면 여기서 처리)
        // (예시: int currentExp = PlayerPrefs.GetInt("PlayerExp", 0)...)

        PlayerPrefs.Save(); // 저장 필수!

        // 결과창 표시
        resultPanel.SetActive(true);
        rankText.text = $"Rank: {rank}";
        rewardText.text = $"보상: {goldReward} G";
    }

    public void OnClickReturnLobby()
    {
        // 로비 씬 이름이 "Lobby"라고 가정
        SceneManager.LoadScene("Main Menu");
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}