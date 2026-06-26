using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;

    public GameObject statusPanel;
    [HideInInspector]public UI_Status UI_Status;
    public UI_StatusBar UI_StatusBar;
    public Enemy_HpBar enemy_HpBar;
    public Character_Stat activeCharacterStat;
    public GameObject inventoryPanel;
    public UI_Inventory UI_Inventory;
    public GameObject messagePanel;
    public Text messageText;
    public GameObject localMapPanel;
    public GameObject questPanel;
    public UI_QuestPanel UI_QuestPanel;
    public GameObject optionPanel;
    public GameObject partyFormationPanel;
    public Text phoneTimeText;

    [Header("Main HUD Elements")]
    public GameObject characterIconPanel;
    public GameObject skillSlotPanel;
    public GameObject minimapPanel;
    public GameObject questTracker;

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
        
        if (UI_QuestPanel == null && questPanel != null)
        {
            UI_QuestPanel = questPanel.GetComponent<UI_QuestPanel>();
        }

        if (partyFormationPanel != null)
        {
            partyFormationPanel.SetActive(false);
        }

        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }

        //UpdateCursorState();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Update()
    {
        if (optionPanel != null && optionPanel.activeSelf)
        {
            UpdatePhoneClock();
        }

        if (!IsUIOpen && (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.LeftAlt)))
        {
            UpdateCursorState();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleQuestPanel();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCharacterInfoPanel();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleLocalMapPanel();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventoryPanel();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            TogglePartyFormationPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIStack.Count > 0)
            {
                GameObject topUI = UIStack.Peek();

                // 로컬 맵인 경우 추가 정리 로직
                if (topUI == localMapPanel)
                {
                    LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
                    if (controller != null) controller.CloseLocalMap();
                }

                if (topUI == partyFormationPanel)
                {
                    UI_PartyFormation formationScript = partyFormationPanel.GetComponent<UI_PartyFormation>();
                    if (formationScript != null) formationScript.SaveAndClose();
                }

                CloseTopUI();
            }
            else
            {
                if (optionPanel != null)
                {
                    ToggleMainHUD(false); // 옵션창 열 때 메인 HUD 끄기
                    OpenUI(optionPanel);
                }
            }
        }

        if (activeCharacterStat != null)
        {
            if (UI_StatusBar != null) UI_StatusBar.UpdateStatus(activeCharacterStat);
        }
        
        //Enemy_HpBar.UpdateStatus(EnemyStat);
    }

    private void UpdatePhoneClock()
    {
        if (phoneTimeText != null)
        {
            phoneTimeText.text = System.DateTime.Now.ToString("HH:mm");
        }
    }

    //메인 HUD끄고 켜기
    public void ToggleMainHUD(bool show)
    {
        if (UI_StatusBar != null) UI_StatusBar.gameObject.SetActive(show);
        if (characterIconPanel != null) characterIconPanel.SetActive(show);
        if (skillSlotPanel != null) skillSlotPanel.SetActive(show);
        if (minimapPanel != null) minimapPanel.SetActive(show);
        if (questTracker != null) questTracker.SetActive(show);
    }

    private void FindLocalPlayerStat()
    {
        Character_Stat[] allStats = FindObjectsOfType<Character_Stat>();

        foreach (var stat in allStats)
        {
            // 씬에 여러 캐릭터가 소환되어 있어도, 현재 활성화(켜진) 캐릭터만 추적
            if (stat.gameObject.activeInHierarchy && stat.CompareTag("Player"))
            {
                activeCharacterStat = stat;
                break;
            }
        }
    }

    public void UpdatePlayerStatus(Character_Stat stat = null) // 매개변수 타입 변경
    {
        if (stat != null) activeCharacterStat = stat;
        if (activeCharacterStat == null) FindLocalPlayerStat();

        // UI_StatusBar에 정보 갱신 요청
        if (activeCharacterStat != null && UI_StatusBar != null)
        {
            UI_StatusBar.UpdateStatus(activeCharacterStat);
        }
    }

    public void OpenUI(GameObject panel)
    {
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
            UIStack.Push(panel);
            Time.timeScale = 0f;
            UpdateCursorState();

            RectTransform rect = panel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();

                Vector2 targetPos = rect.anchoredPosition;
                rect.anchoredPosition = targetPos + new Vector2(0, -200f);
                rect.localScale = Vector3.zero; // 크기 0에서 시작

                rect.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
                rect.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }
    }

    public void CloseTopUI()
    {
        if (UIStack.Count > 0)
        {
            GameObject topUI = UIStack.Pop();
            topUI.transform.DOKill();
            topUI.SetActive(false);
            CheckTimeScale();
            UpdateCursorState();

            if (topUI == optionPanel)
            {
                ToggleMainHUD(true);
            }
        }
    }

    public void CloseSpecificUI(GameObject panel)
    {
        panel.transform.DOKill();
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

            CheckTimeScale();
            UpdateCursorState();
        }

        if (panel == optionPanel)
        {
            ToggleMainHUD(true);
        }
    }

    public void ToggleQuestPanel()
    {
        if (questPanel.activeSelf)
        {
            // 켜져있으면 -> 닫기
            CloseSpecificUI(questPanel);
        }
        else if (!IsUIOpen)
        {
            // 꺼져있으면 -> 열기
            OpenUI(questPanel);
        }
    }

    public void ToggleInventoryPanel()
    {
        if (inventoryPanel.activeSelf)
        {
            CloseSpecificUI(inventoryPanel);
        }
        else if (!IsUIOpen)
        {
            UI_Inventory.RefreshUI();
            OpenUI(inventoryPanel);
        }
    }

    public void ToggleCharacterInfoPanel()
    {
        if (statusPanel.activeSelf)
        {
            CloseSpecificUI(statusPanel);
        }
        else if (!IsUIOpen)
        {
            UpdatePlayerStatus();
            OpenUI(statusPanel);
        }
    }

    public void ToggleLocalMapPanel()
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
            else if (!IsUIOpen)
            {
                OpenUI(localMapPanel);

                LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
                if (controller != null)
                {
                    controller.OpenLocalMap();
                }
            }
        }
    }

    public void TogglePartyFormationPanel()
    {
        if (partyFormationPanel != null)
        {
            if (partyFormationPanel.activeSelf)
            {
                UI_PartyFormation formationScript = partyFormationPanel.GetComponent<UI_PartyFormation>();
                if (formationScript != null)
                {
                    formationScript.SaveAndClose();
                }
                CloseSpecificUI(partyFormationPanel);
            }
            else if (!IsUIOpen)
            {
                OpenUI(partyFormationPanel);
                UI_PartyFormation formationScript = partyFormationPanel.GetComponent<UI_PartyFormation>();
                if (formationScript != null)
                {
                    formationScript.OpenFormationWindow();
                }
            }
        }
    }

    public void ShowMessage(string msg)
    {
        if (messagePanel != null)
        {
            if (hideMessageCoroutine != null)
            {
                StopCoroutine(hideMessageCoroutine);
            }
            messagePanel.SetActive(true);
            messageText.text = msg;

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
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    private void CheckTimeScale()
    {
        if (UIStack.Count == 0)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
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

    public void OnClickGameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
