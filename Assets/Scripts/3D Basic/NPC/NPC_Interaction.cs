using UnityEngine;
using UnityEngine.UI;

public class NPC_Interaction : Interactable
{
    [Header("일반 NPC 전용")]
    public GameObject interactionMenuPanel;

    [Header("NPC 데이터")]
    public NPC_Data npcData;
    public Conversation conversation;

    [Header("UI 버튼")]
    public Button talkButton;
    public Button giftButton;
    public Button closeButton;

    void Start()
    {
        if (interactionMenuPanel != null)
        {
            interactionMenuPanel.SetActive(false);
        }

        if (talkButton != null) talkButton.onClick.AddListener(OnTalk);
        if (giftButton != null) giftButton.onClick.AddListener(OnGiveGift);
        if (closeButton != null) closeButton.onClick.AddListener(Closemenu);
    }

    //메뉴 관리
    protected override void OpenMenu()
    {
        base.OpenMenu(interactionMenuPanel);
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

        Conversation convoToStart = conversation ?? npcData.startingConversation;
        if (convoToStart != null)
        {
            DialogueManager.instance.StartConversation(convoToStart, npcData);
        }
        else
        {
            Debug.LogWarning("이 NPC에 대화가 없습니다.");
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
}
