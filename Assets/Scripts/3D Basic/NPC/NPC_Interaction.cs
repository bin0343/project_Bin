using UnityEngine;
using UnityEngine.UI;

public class NPC_Interaction : Interactable
{
    [Header("일반 NPC 전용")]
    public GameObject interactionMenuPanel;

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
        Debug.Log("대화를 시작합니다.");
        Closemenu();
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
