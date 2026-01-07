using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    public GameObject StatusPanel;
    public UI_Status UI_Status;
    public UI_StatusBar UI_StatusBar;
    public Enemy_HpBar Enemy_HpBar;
    public Player_Stat PlayerStat;
    public GameObject InventoryPanel;
    public UI_Inventory UI_Inventory;
    public GameObject MessagePanel;
    public Text MessageText;
    public GameObject LootPanel;
    public UI_Loot UI_Loot;
    public GameObject localMapPanel;
    public bool isBattleMode { get; set; } = false;

    [Header("메시지 설정")]
    public float messageDisplayTime = 2.0f; //메시지 표시 시간
    private Coroutine hideMessageCoroutine;

    private Stack<GameObject> UIStack = new Stack<GameObject>();

    public bool IsUIOpen => UIStack.Count > 0;

    public bool IsInTargetingMode { get; set; } = false;

    private void Start()
    {
        FindLocalPlayerStat();
        if (UI_Loot == null && LootPanel != null)
        {
            UI_Loot = LootPanel.GetComponent<UI_Loot>();
        }

        //UpdateCursorState();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!IsUIOpen && (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.LeftAlt)))
        {
            UpdateCursorState();
        }


        if (Input.GetKeyDown(KeyCode.U))
        {
            if (StatusPanel.activeSelf)
            {
                CloseSpecificUI(StatusPanel);
            }
            else
            {
                UpdatePlayerStatus();
                OpenUI(StatusPanel);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (localMapPanel != null)
            {
                if (localMapPanel.activeSelf)
                {
                    CloseSpecificUI(localMapPanel);
                    LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
                    if (controller != null)
                    {
                        controller.CloseLocalMap();
                    }
                }
                else
                {
                    // 1. UI 매니저 스택에 추가 및 활성화
                    OpenUI(localMapPanel);

                    // 2. 맵 컨트롤러의 열기 로직 실행 (시간 정지, 버튼 포커스 등)
                    LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
                    if (controller != null)
                    {
                        controller.OpenLocalMap();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (InventoryPanel.activeSelf)
            {
                CloseSpecificUI(InventoryPanel);
            }
            else
            {
                UI_Inventory.RefreshUI();
                OpenUI(InventoryPanel);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIStack.Count > 0 && UIStack.Peek() == localMapPanel)
            {
                LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
                if (controller != null) controller.CloseLocalMap();
            }
            CloseTopUI();
        }
        UI_StatusBar.UpdateStatus(PlayerStat);
        //Enemy_HpBar.UpdateStatus(EnemyStat);
    }

    private void FindLocalPlayerStat()
    {
        // 씬에 있는 모든 Player_Stat을 다 뒤짐
        Player_Stat[] allStats = FindObjectsOfType<Player_Stat>();

        foreach (var stat in allStats)
        {
            // Global 데이터(매니저)가 아닌 녀석을 발견하면 그게 진짜 캐릭터임
            if (!stat.isGlobalData)
            {
                PlayerStat = stat;
                break; // 찾았으면 반복 종료
            }
        }
    }

    public void UpdatePlayerStatus(Player_Stat stat = null)
    {
        // 1. 외부에서 직접 찔러준 경우 (가장 확실함)
        if (stat != null && !stat.isGlobalData)
        {
            PlayerStat = stat;
        }

        // 2. 아직도 누군지 모르거나, 알고 있는 애가 Global 놈이라면? -> 다시 찾아!
        if (PlayerStat == null || PlayerStat.isGlobalData)
        {
            FindLocalPlayerStat();
        }

        // 3. 찾은 진짜 캐릭터로 UI 갱신
        if (PlayerStat != null && !PlayerStat.isGlobalData)
        {
            if (UI_Status != null) UI_Status.UpdateStatus(PlayerStat);
            if (UI_StatusBar != null) UI_StatusBar.UpdateStatus(PlayerStat);
        }
    }

    public void OpenUI(GameObject panel)
    {
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
            UIStack.Push(panel);
            UpdateCursorState();
        }
    }

    public void CloseTopUI()
    {
        if (UIStack.Count > 0)
        {
            GameObject topUI = UIStack.Pop();
            topUI.SetActive(false);
            UpdateCursorState();
        }
    }

    public void CloseSpecificUI(GameObject panel)
    {
        panel.SetActive(false); // 일단 끈다.

        if (UIStack.Contains(panel))
        {
            Stack<GameObject> tempStack = new Stack<GameObject>();
            while (UIStack.Count > 0)
            {
                GameObject top = UIStack.Pop();
                if (top != panel)
                    tempStack.Push(top);
            }

            while (tempStack.Count > 0)
            {
                UIStack.Push(tempStack.Pop());
            }

            UpdateCursorState();
        }
    }

    public void ShowMessage(string msg)
    {
        if (MessagePanel != null)
        {
            if (hideMessageCoroutine != null)   //다른 메시지 코루틴 진행중이면 그 코루틴 중지
            {
                StopCoroutine(hideMessageCoroutine);
            }
            MessagePanel.SetActive(true);
            MessageText.text = msg;

            hideMessageCoroutine = StartCoroutine(HideMessageRoutine(messageDisplayTime));
        }
    }

    private IEnumerator HideMessageRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        HideMessage();
        hideMessageCoroutine = null;
    }

    public void HideMessage()
    {
        if (MessagePanel != null)
        {
            MessagePanel.SetActive(false);
        }
    }

    public void UpdateCursorState()
    {
        // UI 창이 열려있거나, '스킬 조준 모드'일 경우
        if (IsUIOpen || IsInTargetingMode || isBattleMode)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else // UI도 닫혀있고 조준 모드도 아닐 경우
        {
            // Alt 키를 누르고 있을 때만 커서를 보여줌
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            // 그 외에는 커서를 숨기고 잠금
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void SetBattleMode(bool isBattle)
    {
        isBattleMode = isBattle;
        UpdateCursorState(); // 모드가 바뀌었으니 커서 상태도 바로 갱신
    }

    #region Cursor Management
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideCursor()
    {
        UpdateCursorState();
    }
    #endregion
}
