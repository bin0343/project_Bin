using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NPC_Interaction : Interactable
{
    [Header("일반 NPC 전용")]
    public GameObject interactionMenuPanel;

    [Header("NPC 데이터")]
    public NPC_Data npcData;
    public Conversation conversation;   //일반대화
    public Conversation pendingConversation;
    private Quest pendingQuest;

    [Header("UI 버튼")]
    public Button talkButton;
    public Button giftButton;
    public Button closeButton;
    public Button questButton;

    public Text questButtonText;

    void Start()
    {
        if (interactionMenuPanel != null)
        {
            interactionMenuPanel.SetActive(false);
        }

        if (talkButton != null) talkButton.onClick.AddListener(OnTalk);
        if (giftButton != null) giftButton.onClick.AddListener(OnGiveGift);
        if (closeButton != null) closeButton.onClick.AddListener(Closemenu);
        if (questButton != null) questButton.onClick.AddListener(OnQuestButton);
    }

    //메뉴 관리
    protected override void OpenMenu()
    {
        base.OpenMenu(interactionMenuPanel);

        CheckForQuests();
    }

    //버튼 클릭 이벤트
    public void OnTalk()
    {
        Debug.Log("대화하기를 선택했습니다.");
        if (npcData == null)
        {
            Debug.LogWarning("Npc 데이터가 없습니다.");
            return;
        }
        Closemenu();

        /*Conversation convoToStart = DetermineConversation();
        if (convoToStart != null)
        {
            DialogueManager.instance.StartConversation(convoToStart, npcData);
        }
        else
        {
            Debug.LogWarning("이 NPC에 대화가 없습니다.");
        }*/
        Conversation convo = conversation ?? npcData.startingConversation;
        if (convo != null)
        {
            DialogueManager.instance.StartConversation(convo, npcData);
        }
        else
        {
            Debug.LogWarning("이 NPC에 할당된 일반 대화가 없습니다.");
        }
    }

    public void OnGiveGift()
    {
        Debug.Log("선물하기 창을 엽니다. (인벤토리 연동)");
        Closemenu();
    }

    public void OnCloseMenu()
    {
        Debug.Log("메뉴를 닫습니다.");
        //CloseInteractionMenu();
    }

    private Conversation DetermineConversation()
    {
        // (간단하게, 이 NPC가 주는 첫 번째 퀘스트만 확인)
        Quest quest = npcData.availableQuests.FirstOrDefault();

        if (quest != null)
        {
            QuestStatus status = QuestManager.instance.GetQuestStatus(quest.questID);

            if (status == QuestStatus.COMPLETED && npcData.questCompleteConversation != null)
            {
                // 1. 완료 가능 상태 -> 완료 대화
                return npcData.questCompleteConversation;
            }
            else if (status == QuestStatus.IN_PROGRESS && npcData.questInProgressConversation != null)
            {
                // 2. 진행 중 상태 -> 진행 중 대화
                return npcData.questInProgressConversation;
            }
            // 3. 수락 전 또는 완료 후 -> 기본 대화 (기본 대화에서 퀘스트 수락이 이뤄짐)
        }

        // 퀘스트가 없거나, 수락 전이거나, 완료 후일 때
        return conversation ?? npcData.startingConversation;
    }

    private void CheckForQuests()
    {
        if (npcData == null || questButton == null) return;

        pendingQuest = null;
        pendingConversation = null;
        bool showQuestButton = false;

        foreach (var quest in npcData.availableQuests)
        {
            if (QuestManager.instance.GetQuestStatus(quest.questID) ==QuestStatus.COMPLETED)
            {
                pendingQuest = quest;
                pendingConversation = quest.completeConversation;
                if (questButtonText != null) questButtonText.text = "퀘스트 완료";
                showQuestButton = true;
                break;
            }
        }

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

        if (showQuestButton)
        {
            questButton.transform.SetAsFirstSibling();
        }
    }

    public void OnQuestButton()
    {
        if (pendingQuest != null && pendingConversation != null)
        {
            Debug.Log($"퀘스트 관련 대화 시작: {pendingQuest.questTitle}");
            Closemenu();

            DialogueManager.instance.StartConversation(pendingConversation, npcData);
        }
        else
        {
            Debug.LogWarning("실행할 퀘스트 대화가 없습니다.");
        }
    }
}
