using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour
{
    [Header("상호작용 설정")]
    public KeyCode interactionKey = KeyCode.E;

    protected bool isPlayerInRange = false;   //플레이어가 범위 안에 있는지 확인
    protected bool isMenuOpen = false;

    protected GameObject currentOpenMenu = null;

    public GameObject interactionPromptUI;  //상호작용 가능한지 알리는 UI (예 : "E"키 아이콘)
    public Text interactionText;

    void Update()
    {
        if (isPlayerInRange && !isMenuOpen && Input.GetKeyDown(interactionKey))
        {
            OpenMenu();
        }
        if (isPlayerInRange && !isMenuOpen && interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape)) isMenuOpen = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(true);
            }

            if (interactionText != null)
            {
                interactionText.text = $"{interactionKey} : 대화";
            }

            Debug.Log("플레이어 범위 진입");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionPromptUI != null) interactionPromptUI.SetActive(false);
            Debug.Log("플레이어 범위 이탈");
        }
    }

    //메뉴 관리
    protected void OpenMenu(GameObject menuPanel)
    {
        isMenuOpen = true;
        currentOpenMenu = menuPanel;
        // currentOpenMenu.SetActive(true);

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }

        //Time.timeScale = 0f; //게임 일시정지(향후 게임시간은 두고 플레이어 조작만 막기)
        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.OpenUI(currentOpenMenu);
        }
        else
        {
            currentOpenMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
    }

    public virtual void Closemenu()
    {
        isMenuOpen = false;
        
        //currentOpenMenu.SetActive(false);
        //currentOpenMenu = null;

        //Time.timeScale = 1f;
        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.CloseSpecificUI(currentOpenMenu);
        }
        else
        {
            if (currentOpenMenu != null) currentOpenMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        currentOpenMenu = null;
    }

    protected abstract void OpenMenu();
}
