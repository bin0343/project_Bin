using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("--- UI 연결: 기존 대화창 재사용 ---")]
    public GameObject dialoguePanel;
    public Text txtName;
    public Text txtDialogue;
    public GameObject choiceGroup;

    public Image standingCG;
    public Sprite assistantSprite;

    [Header("--- UI 연결: 이름 입력 ---")]
    public GameObject nameInputPanel;
    public InputField inputName;
    public Button btnConfirm;

    [Header("--- UI 연결: 인트로 전용 클릭 ---")]
    public Button introNextButton;

    [Header("--- 매니저 연결 ---")]
    public LobbyManager lobbyManager;
    // [추가] 기존 대화 매니저를 잠재우기 위해 연결
    public DialogueManager originDialogueManager;

    private int step = 0;

    void Start()
    {
        if (PlayerPrefs.GetInt("IsFirstVisit", 0) == 1)
        {
            StartIntro();
        }
        else
        {
            // 인트로가 아니면 얌전히 꺼지기
            if (nameInputPanel) nameInputPanel.SetActive(false);
            if (introNextButton) introNextButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        if (btnConfirm) btnConfirm.onClick.AddListener(OnClickConfirmName);
        if (introNextButton) introNextButton.onClick.AddListener(OnClickNextDialogue);
    }

    void StartIntro()
    {
        if (lobbyManager != null) lobbyManager.SetLobbyUIVisible(false);
        if (originDialogueManager != null) originDialogueManager.enabled = false;

        dialoguePanel.SetActive(true);
        if (choiceGroup != null) choiceGroup.SetActive(false);
        nameInputPanel.SetActive(false);

        if (introNextButton) introNextButton.gameObject.SetActive(true);

        if (standingCG != null && assistantSprite != null)
        {
            standingCG.sprite = assistantSprite;
            standingCG.gameObject.SetActive(true);
            standingCG.preserveAspect = true;
        }

        txtName.text = "조교";
        step = 0;
        NextDialogue();
    }

    public void OnClickNextDialogue()
    {
        if (step == 4) return;
        NextDialogue();
    }

    void NextDialogue()
    {
        if (choiceGroup != null) choiceGroup.SetActive(false);

        step++;

        if (step == 1) txtDialogue.text = "신입생 환영회에 온 것을 환영한다.";
        else if (step == 2) txtDialogue.text = "아카데미 입학 처리를 위해 서류를 작성해야 해.";
        else if (step == 3) txtDialogue.text = "자네의 이름은 무엇인가?";
        else if (step == 4)
        {
            nameInputPanel.SetActive(true);
        }
    }

    void OnClickConfirmName()
    {
        string playerName = inputName.text.Trim();
        if (string.IsNullOrEmpty(playerName) || playerName.Length > 8) return;

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("IsFirstVisit", 0);
        PlayerPrefs.Save();

        nameInputPanel.SetActive(false);

        txtName.text = "조교";
        txtDialogue.text = $"{playerName}? 흐음... 기억해두지.\n이제 자유롭게 활동해라.";

        Invoke("EndIntro", 2.5f);
    }

    void EndIntro()
    {
        dialoguePanel.SetActive(false);
        if (introNextButton) introNextButton.gameObject.SetActive(false);

        if (originDialogueManager != null) originDialogueManager.enabled = true;

        if (lobbyManager != null)
        {
            lobbyManager.RefreshUserInfo();
            lobbyManager.SetLobbyUIVisible(true);
        }
        if (standingCG != null) standingCG.sprite = null;
    }
}