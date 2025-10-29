using UnityEngine;
using UnityEngine.UI;

public class NPC_Interaction : MonoBehaviour
{
    [Header("상호작용 설정")]
    public KeyCode interactionKey = KeyCode.E;
    public GameObject interactionMenyPanel;

    [Header("UI 버튼")]
    public Button talkButton;
    public Button giftButton;
    public Button closeButton;

    private bool isPlayerInRange = false;   //플레이어가 범위 안에 있는지 확인
    private bool isMenuOpen = false;

    //public GameObject interactionPromptUI;  //상호작용 가능한지 알리는 UI (예 : "E"키 아이콘)

    void Start()
    {
        if (interactionMenyPanel != null)
        {
            interactionMenyPanel.SetActive(false);
        }

        if (talkButton != null) talkButton.onClick.AddListener(OnTalk);
    }

    void Update()
    {
        if (isPlayerInRange && !isMenuOpen && Input.GetKeyDown(interactionKey))
        {
            OpenInteractionMenu();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            //if (interactionPromptUI != null) interactionPromptUI.SetActive(true);
            Debug.Log("플레이어 범위 진입");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            //if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
            Debug.Log("플레이어 범위 이탈");
        }
    }

    //메뉴 관리
    private void OpenInteractionMenu()
    {
        isMenuOpen = true;
        interactionMenyPanel.SetActive(true);

        Time.timeScale = 0f; //게임 일시정지(향후 게임시간은 두고 플레이어 조작만 막기)

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseInteractionMenu()
    {
        isMenuOpen = false;
        interactionMenyPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //버튼 클릭 이벤트
    public void OnTalk()
    {
        Debug.Log("대화를 시작합니다.");
        CloseInteractionMenu();
    }

    public void OnGiveGift()
    {
        Debug.Log("선물하기 창을 엽니다. (인벤토리 연동)");
        CloseInteractionMenu();
    }

    public void OnCloseMenu()
    {
        Debug.Log("메뉴를 닫습니다.");
        CloseInteractionMenu();
    }
}
