using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// Interactable 상속 제거 (이제 UI 버튼으로만 작동)
public class NPC_Interaction : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject interactionMenuPanel; // 대화/선물/퀘스트 버튼이 있는 패널

    [Header("NPC 데이터")]
    public NPC_Data npcData;

    [Header("대화 데이터")]
    public Conversation greetingConversation; // "안녕? 무슨 일이야?" (선택지: 대화하기 / 나가기)
    public Conversation normalConversation;   // 실제 "대화하기" 눌렀을 때 내용

    private Conversation pendingConversation;
    private Quest pendingQuest;

    [Header("메뉴 버튼")]
    public Button talkButton;
    public Button giftButton;
    //public Button closeButton;
    public Button questButton;
    public Text questButtonText;

    private void Start()
    {
        // 이 스크립트가 붙은 오브젝트(NPC 그림) 자체가 버튼 역할을 함
        Button myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.RemoveAllListeners();
            myButton.onClick.AddListener(OnClickNPC);
        }

        // 메뉴 패널 버튼 연결
        if (talkButton != null) talkButton.onClick.AddListener(OnTalk);
        if (giftButton != null) giftButton.onClick.AddListener(OnGiveGift);
        //if (closeButton != null) closeButton.onClick.AddListener(CloseMenu);
        if (questButton != null) questButton.onClick.AddListener(OnQuestButton);

        // 시작 시 메뉴 패널 끄기
        if (interactionMenuPanel != null) interactionMenuPanel.SetActive(false);
    }

    // 1. NPC 버튼 클릭 시 -> 인사 대화 실행
    public void OnClickNPC()
    {
        if (npcData == null) return;

        // greetingConversation이 없으면 데이터의 기본 대화 사용
        Conversation startConvo = greetingConversation != null ? greetingConversation : npcData.startingConversation;

        // 대화 매니저에게 "내가(this) 대화 요청했다"고 알림
        //DialogueManager.instance.StartConversation(startConvo, npcData, this);
    }

    // 2. 대화 매니저가 호출해주는 함수 (메뉴판 열기)
    public void ShowInteractionMenu()
    {
        if (interactionMenuPanel != null)
        {
            CheckForQuests(); // 퀘스트 상태 확인
            UI_Manager.Instance.OpenUI(interactionMenuPanel);
        }
    }

    // 3. 메뉴 닫기
    public void CloseMenu()
    {
        if (interactionMenuPanel != null)
            UI_Manager.Instance.CloseSpecificUI(interactionMenuPanel);
    }

    // --- 이하 버튼 기능 ---

    public void OnTalk()
    {
        CloseMenu(); // 메뉴 닫고 실제 대화 시작

        Conversation convo = normalConversation ?? npcData.startingConversation;
        if (convo != null)
        {
            // 여기서는 메뉴를 또 열 필요가 없으므로 this를 넘기지 않거나, 
            // 선택지에 openInteractionMenu가 false여야 함
            //DialogueManager.instance.StartConversation(convo, npcData, null);
        }
    }

    public void OnGiveGift()
    {
        Debug.Log("선물하기 UI 열기");
        CloseMenu();
    }

    // 퀘스트 버튼 로직 (기존 유지)
    private void CheckForQuests()
    {
        if (questButton == null) return;

        pendingQuest = null;
        pendingConversation = null;
        bool showQuestButton = false;

        // 완료 가능한 퀘스트 확인
        foreach (var quest in npcData.availableQuests)
        {
            if (QuestManager.instance.GetQuestStatus(quest.questID) == QuestStatus.COMPLETED)
            {
                pendingQuest = quest;
                pendingConversation = quest.completeConversation;
                if (questButtonText != null) questButtonText.text = "퀘스트 완료";
                showQuestButton = true;
                break;
            }
        }

        // 받을 수 있는 퀘스트 확인
        if (!showQuestButton)
        {
            foreach (var quest in npcData.availableQuests)
            {
                if (QuestManager.instance.GetQuestStatus(quest.questID) == QuestStatus.NOT_STARTED)
                {
                    pendingQuest = quest;
                    pendingConversation = quest.startConversation;
                    if (questButtonText != null) questButtonText.text = "퀘스트 받기";
                    showQuestButton = true;
                    break;
                }
            }
        }

        questButton.gameObject.SetActive(showQuestButton);
        if (showQuestButton) questButton.transform.SetAsFirstSibling();
    }

    public void OnQuestButton()
    {
        if (pendingConversation != null)
        {
            CloseMenu();
            //
            //DialogueManager.instance.StartConversation(pendingConversation, npcData, null);
        }
    }
}