using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[System.Serializable]
public class IntroData
{
    public int id;
    public string speaker;
    public string text;
    public string eventType;
}

public class IntroManager : MonoBehaviour
{
    public static IntroManager instance;

    [Header("--- 데이터 파일 ---")]
    public TextAsset tutorialCsv;
    public Player_Data playerData;

    [Header("--- UI 연결 ---")]
    public GameObject dialoguePanel;
    public Text txtName;
    public Text txtDialogue;
    public GameObject choiceGroup;

    public Image standingCG;
    public Sprite assistantSprite;

    [Header("--- UI 연결: 기능 ---")]
    public GameObject nameInputPanel;
    public InputField inputName;
    public Button btnConfirm;
    public Button introNextButton;

    [Header("--- 매니저 연결 ---")]
    public LobbyManager lobbyManager;
    public DialogueManager originDialogueManager;

    private List<IntroData> introDataList = new List<IntroData>();
    private int currentIndex = 0;

    // [상태 변수]
    public bool isScheduleGuidePhase = false;   // 스케줄 버튼 누르기 대기 중
    public bool isForcingBuilding = false;      // 건물 강제 클릭 모드
    public int targetBuildingID = 0;            // 목표 건물 ID

    //private bool isMapTutorialActive = false;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[IntroManager] 씬 로드됨: {scene.name}");

        RefreshReferences();

        CheckTutorialStatus();
    }

    void RefreshReferences()
    {
        if (lobbyManager == null) lobbyManager = FindObjectOfType<LobbyManager>();

        if (originDialogueManager == null) originDialogueManager = FindObjectOfType<DialogueManager>();

        if (introNextButton == null)
        {
            GameObject btnObj = GameObject.Find("IntroNextButton");
            if (btnObj != null) introNextButton = btnObj.GetComponent<Button>();
        }

        if (dialoguePanel == null)
        {
            GameObject panelObj = GameObject.Find("DialoguePanel");
            if (panelObj != null) dialoguePanel = panelObj;
        }

        if (btnConfirm != null)
        {
            btnConfirm.onClick.RemoveAllListeners();
            btnConfirm.onClick.AddListener(OnClickConfirmName);
        }
        if (introNextButton != null)
        {
            introNextButton.onClick.RemoveAllListeners();
            introNextButton.onClick.AddListener(OnClickNextDialogue);
        }
    }

    void CheckTutorialStatus()
    {
        if (introDataList.Count == 0) ParseCSV();

        bool isFinished = false;
        if (GameDataManager.Instance != null)
        {
            isFinished = GameDataManager.Instance.saveData.isTutorialFinished;
        }

        Debug.Log($"[IntroManager] 튜토리얼 완료 여부: {isFinished}");

        if (isFinished)
        {
            DisableAllIntroUI();
        }
        else
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Main Menu" || sceneName == "Lobby")
            {
                if (currentIndex == 0 && !dialoguePanel.activeSelf)
                {
                    StartIntro();
                }
            }
            else
            {
                DisableAllIntroUI();
            }
        }
    }

    public void DisableAllIntroUI()
    {
        if (introNextButton != null) introNextButton.gameObject.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (standingCG != null) standingCG.gameObject.SetActive(false);

        if (originDialogueManager != null) originDialogueManager.enabled = true;
    }

    void ParseCSV()
    {
        if (tutorialCsv == null) return;
        string[] lines = tutorialCsv.text.Replace("\r\n", "\n").Split('\n');
        string pattern = @",(?=(?:[^""]*""[^""]*"")*[^""]*$)";

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = Regex.Split(lines[i], pattern);
            if (cols.Length < 4) continue;

            IntroData data = new IntroData();
            data.id = int.Parse(cols[0]);
            data.speaker = cols[1];
            data.text = cols[2].Replace("\"", "").Replace("\\n", "\n");
            data.eventType = cols[3].Trim();
            introDataList.Add(data);
        }
    }

    void StartIntro()
    {
        if (lobbyManager != null) lobbyManager.SetLobbyUIVisible(false);
        if (originDialogueManager != null) originDialogueManager.enabled = false;

        dialoguePanel.SetActive(true);
        if (choiceGroup != null) choiceGroup.SetActive(false);
        if (nameInputPanel) nameInputPanel.SetActive(false);

        if (introNextButton) introNextButton.gameObject.SetActive(true);

        if (standingCG != null && assistantSprite != null)
        {
            standingCG.sprite = assistantSprite;
            standingCG.gameObject.SetActive(true);
            standingCG.preserveAspect = true;
        }

        currentIndex = 0;
        ShowDialogue();
    }

    void ShowDialogue()
    {
        if (currentIndex >= introDataList.Count)
        {
            EndIntro();
            return;
        }

        IntroData data = introDataList[currentIndex];

        txtName.text = data.speaker;
        string processedText = data.text.Replace("{NAME}", PlayerPrefs.GetString("PlayerName", "신입생"));
        txtDialogue.text = processedText;

        switch (data.eventType)
        {
            case "NAME_INPUT":
                if (nameInputPanel) nameInputPanel.SetActive(true);
                break;
            case "SCHEDULE_GUIDE":
                GuideToSchedule();
                break;
            case "FORCE_MAIN_BUILDING":
                isForcingBuilding = true;
                targetBuildingID = 101;
                break;
            case "EXIT": // 종료 이벤트 처리 (필요 시)
                break;
        }
    }

    public void OnClickNextDialogue()
    {
        if (currentIndex < introDataList.Count && introDataList[currentIndex].eventType == "NAME_INPUT") return;
        if (isScheduleGuidePhase) return;

        if (isForcingBuilding)
        {
            CloseDialogueForBuildingClick();
            return;
        }

        currentIndex++;
        ShowDialogue();
    }

    void OnClickConfirmName()
    {
        string playerName = inputName.text.Trim();
        if (string.IsNullOrEmpty(playerName) || playerName.Length > 8) return;

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("IsFirstVisit", 0);
        PlayerPrefs.Save();

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.saveData.playerName = playerName;
        }

        if (playerData != null)
        {
            playerData.characterName = playerName;
        }

        nameInputPanel.SetActive(false);
        currentIndex++;
        ShowDialogue();
    }

    void GuideToSchedule()
    {
        if (standingCG != null) standingCG.gameObject.SetActive(false);
        if (lobbyManager != null) lobbyManager.ShowOnlyScheduleButton();

        isScheduleGuidePhase = true;
        Invoke("CloseDialogueForInput", 1.5f);
    }

    void CloseDialogueForInput()
    {
        dialoguePanel.SetActive(false);
        if (introNextButton) introNextButton.gameObject.SetActive(false);
    }

    public void StartMapExplanation()
    {
        if (!isScheduleGuidePhase) return;

        isScheduleGuidePhase = false;

        //isMapTutorialActive = true;

        dialoguePanel.SetActive(true);
        if (standingCG) standingCG.gameObject.SetActive(true);
        if (introNextButton) introNextButton.gameObject.SetActive(true);

        int mapStartIndex = introDataList.FindIndex(x => x.id == 10);
        if (mapStartIndex != -1)
        {
            currentIndex = mapStartIndex;
            ShowDialogue();
        }
    }

    void CloseDialogueForBuildingClick()
    {
        dialoguePanel.SetActive(false);
        if (introNextButton) introNextButton.gameObject.SetActive(false);
    }

    public bool CheckBuildingClick(int clickedID)
    {
        if (!isForcingBuilding) return true;

        if (clickedID == targetBuildingID)
        {
            isForcingBuilding = false;
            // 패널 열리는 시간(0.5초) 뒤에 마지막 대사 출력
            Invoke("StartFinalMessage", 0.5f);
            return true;
        }
        else
        {
            ShowWarningDialogue();
            return false;
        }
    }

    void ShowWarningDialogue()
    {
        dialoguePanel.SetActive(true);
        if (introNextButton) introNextButton.gameObject.SetActive(true);
        ShowDialogue();
    }

    void StartFinalMessage()
    {
        dialoguePanel.SetActive(true);
        if (introNextButton) introNextButton.gameObject.SetActive(true);
        if (standingCG) standingCG.gameObject.SetActive(true);

        int finalIndex = introDataList.FindIndex(x => x.id == 20);
        if (finalIndex != -1)
        {
            currentIndex = finalIndex;
            ShowDialogue();
        }
    }

    public void EndIntro()
    {
        dialoguePanel.SetActive(false);
        if (introNextButton) introNextButton.gameObject.SetActive(false);
        if (standingCG) standingCG.gameObject.SetActive(false);

        if (originDialogueManager != null) originDialogueManager.enabled = true;

        //isMapTutorialActive = false;

        if (lobbyManager != null)
        {
            lobbyManager.RefreshUserInfo();
            lobbyManager.SetLobbyUIVisible(true);
            lobbyManager.RestoreAllBottomButtons();

            if (lobbyManager.globalBackButton) lobbyManager.globalBackButton.SetActive(true);
        }

        isScheduleGuidePhase = false;
        isForcingBuilding = false;

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.saveData.isTutorialFinished = true;
            GameDataManager.Instance.SaveGame();
        }
    }
}