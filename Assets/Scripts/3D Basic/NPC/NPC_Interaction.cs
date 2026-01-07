using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NPC_Interaction : MonoBehaviour
{
    [Header("UI 패널 연결 (Popup Layer)")]
    public GameObject interactionMenuPanel; // 공용 메뉴판

    [Header("NPC 데이터")]
    public NPC_Data npcData;

    [Header("--- [기능 1] 일상 대화 (CSV) 설정 ---")]
    public int csvDialogueID;

    [Header("--- [기능 2] 퀘스트 설정 (자동 감지) ---")]
    private int pendingDialogueID; // 퀘스트용 대화 ID (자동 할당)

    [Header("메뉴판 내부 버튼 연결")]
    public Button talkButton;
    public Button giftButton;
    public Button questButton;
    public Text questButtonText;

    private void Start()
    {
        Button myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.RemoveAllListeners();
            myButton.onClick.AddListener(ShowInteractionMenu);
        }

        if (talkButton != null)
        {
            talkButton.onClick.RemoveAllListeners();
            talkButton.onClick.AddListener(OnTalkClicked);
        }
        if (giftButton != null)
        {
            giftButton.onClick.RemoveAllListeners();
            giftButton.onClick.AddListener(OnGiftClicked);
        }
        if (questButton != null)
        {
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(OnQuestClicked);
        }

        if (interactionMenuPanel != null) interactionMenuPanel.SetActive(false);
    }

    public void ShowInteractionMenu()
    {
        if (interactionMenuPanel != null)
        {
            CheckForQuests();
            interactionMenuPanel.SetActive(true);
        }
    }

    public void CloseMenu()
    {
        if (interactionMenuPanel != null)
        {
            interactionMenuPanel.SetActive(false);
        }
    }

    public void OnTalkClicked()
    {
        CloseMenu();
        if (DialogueManager.instance != null)
        {
            // 일상 대화 시작
            DialogueManager.instance.StartDialogue(csvDialogueID, npcData);
        }
    }

    public void OnGiftClicked()
    {
        Debug.Log("선물하기 창 열기");
        CloseMenu();
        // UI_Gift.Instance.Open(npcData); 
    }

    // [변경 2] 퀘스트 체크 로직에서 ID를 할당하도록 수정
    private void CheckForQuests()
    {
        if (questButton == null) return;

        pendingDialogueID = 0; // 초기화 (0은 대화 없음 의미)
        bool showQuestButton = false;
        string btnText = "";

        // (1) 완료 가능한 퀘스트 찾기
        foreach (var quest in npcData.availableQuests)
        {
            if (QuestManager.instance.GetQuestStatus(quest.questID) == QuestStatus.COMPLETED)
            {
                // Quest ScriptableObject에 'completeDialogueID' 변수가 있어야 함
                pendingDialogueID = quest.completeDialogueID;
                btnText = "보상 받기";
                showQuestButton = true;
                break;
            }
        }

        // (2) 시작 가능한 퀘스트 찾기
        if (!showQuestButton)
        {
            foreach (var quest in npcData.availableQuests)
            {
                if (QuestManager.instance.GetQuestStatus(quest.questID) == QuestStatus.NOT_STARTED)
                {
                    // Quest ScriptableObject에 'startDialogueID' 변수가 있어야 함
                    pendingDialogueID = quest.startDialogueID;
                    btnText = "의뢰 확인";
                    showQuestButton = true;
                    break;
                }
            }
        }

        questButton.gameObject.SetActive(showQuestButton);
        if (showQuestButton && questButtonText != null)
        {
            questButtonText.text = btnText;
        }
    }

    // [변경 3] 퀘스트 버튼 클릭 시 CSV 대화 함수 호출
    public void OnQuestClicked()
    {
        // 0이 아니면 할당된 대화가 있다는 뜻
        if (pendingDialogueID != 0)
        {
            CloseMenu();

            if (DialogueManager.instance != null)
            {
                // StartConversation 대신 StartDialogue 사용 (ID 넘김)
                DialogueManager.instance.StartDialogue(pendingDialogueID, npcData);
            }
        }
    }
}