using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("메인 HUD 캔버스")]
    public Canvas mainHUDCanvas;

    [Header("UI 연결")]
    public GameObject dialogueRoot;
    public Text txtName;
    public Text txtDialogue;
    public Button btnNext;

    [Header("선택지 UI")]
    public GameObject choiceGroup;
    public Button btnAccept;
    public Text txtAccept;
    public Button btnDecline;
    public Text txtDecline;

    public bool isDialogueActive = false;

    private string[] currentLines;
    private int currentLineIndex;
    private string speakerName;

    private string acceptReaction;
    private string declineReaction;
    private Action onAcceptAction;

    private void Awake()
    {
        if (instance == null) instance = this;
        dialogueRoot.SetActive(false);
        choiceGroup.SetActive(false);

        btnNext.onClick.AddListener(NextLine);
        btnAccept.onClick.AddListener(HandleAccept);
        btnDecline.onClick.AddListener(HandleDecline);
    }

    public void StartQuestSequence(string speaker, Quest quest, Action onAccept)
    {
        SetupDialogue(speaker);
        currentLines = quest.startDialogue;
        acceptReaction = quest.acceptedDialogue;
        declineReaction = quest.declinedDialogue;
        onAcceptAction = onAccept;

        txtAccept.text = quest.acceptButtonText;
        txtDecline.text = quest.declineButtonText;

        ShowCurrentLine();
    }

    public void StartNormalSequence(string speaker, string[] lines, Action onComplete = null)
    {
        SetupDialogue(speaker);
        currentLines = lines;
        acceptReaction = null;
        onAcceptAction = onComplete;

        ShowCurrentLine();
    }

    private void SetupDialogue(string speaker)
    {
        isDialogueActive = true;
        dialogueRoot.SetActive(true);
        choiceGroup.SetActive(false);
        btnNext.gameObject.SetActive(true);
        speakerName = speaker;
        txtName.text = speaker;
        currentLineIndex = 0;

        if (mainHUDCanvas != null)
        {
            mainHUDCanvas.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowCurrentLine()
    {
        // 빈 대사열 방어 코드
        if (currentLines == null || currentLines.Length == 0)
        {
            CloseDialogue();
            return;
        }
        txtDialogue.text = currentLines[currentLineIndex];
    }

    private void NextLine()
    {
        currentLineIndex++;
        if (currentLineIndex < currentLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            if (!string.IsNullOrEmpty(acceptReaction))
            {
                btnNext.gameObject.SetActive(false);
                choiceGroup.SetActive(true);
            }
            else
            {
                onAcceptAction?.Invoke();
                CloseDialogue();
            }
        }
    }

    private void HandleAccept()
    {
        choiceGroup.SetActive(false);
        onAcceptAction?.Invoke();
        ShowResultLine(acceptReaction);
    }

    private void HandleDecline()
    {
        choiceGroup.SetActive(false);
        ShowResultLine(declineReaction);
    }

    private void ShowResultLine(string reaction)
    {
        if (string.IsNullOrEmpty(reaction))
        {
            CloseDialogue();
            return;
        }

        txtDialogue.text = reaction;
        btnNext.gameObject.SetActive(true);
        currentLines = new string[] { reaction };
        currentLineIndex = 0;
        acceptReaction = null;
        onAcceptAction = null;
    }

    public void CloseDialogue()
    {
        isDialogueActive = false;
        dialogueRoot.SetActive(false);
        choiceGroup.SetActive(false);

        if (mainHUDCanvas != null)
        {
            mainHUDCanvas.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}