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

        // 1. Global Stat (Manager)에 즉시 반영 -> 로비 UI 갱신용
        Player_Stat globalStat = Player_Stat.globalInstance;
        if (globalStat == null && Player_Inventory.instance != null)
        {
            globalStat = Player_Inventory.instance.GetComponent<Player_Stat>();
        }

        if (globalStat != null)
        {
            globalStat.GainGold(goldReward);
            globalStat.GainExp(expReward);
            Debug.Log($"미니게임 보상 지급 완료: {goldReward}G, {expReward}Exp");
        }
        else
        {
            Debug.LogWarning("Global Player_Stat을 찾을 수 없습니다! 매니저 오브젝트를 확인하세요.");
        }

        // 2. PlayerPrefs 백업 (기존 유지 - 데이터 보존용)
        int currentGold = PlayerPrefs.GetInt("PlayerGold", 0);
        PlayerPrefs.SetInt("PlayerGold", currentGold + goldReward);

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