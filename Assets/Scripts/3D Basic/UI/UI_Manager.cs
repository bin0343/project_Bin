using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    [Header("UI Panels")]
    public GameObject statusPanel;
    [HideInInspector] public UI_Status UI_Status;
    public UI_StatusBar UI_StatusBar;
    public UI_Inventory UI_Inventory;
    public UI_QuestPanel UI_QuestPanel;
    public Enemy_HpBar enemy_HpBar;
    public Character_Stat activeCharacterStat;
    public GameObject blurPanel;
    public GameObject inventoryPanel;
    public GameObject messagePanel;
    public GameObject localMapPanel;
    public GameObject questPanel;
    public GameObject optionPanel;
    public GameObject partyFormationPanel;
    public GameObject QuitPanel;
    public TMP_Text messageText;
    public Text phoneTimeText;

    [Header("공용 상호작용 UI")]
    [SerializeField]
    private GameObject interactionPromptUI;
    [SerializeField]
    private Text interactionPromptText;

    public GameObject InteractionPromptUI
    {
        get { return interactionPromptUI; }
    }

    public Text InteractionPromptText
    {
        get { return interactionPromptText; }
    }

    [Header("Main HUD Elements (Legacy References)")]
    public GameObject characterIconPanel;
    public GameObject skillSlotPanel;
    public GameObject minimapPanel;
    public GameObject questTracker;

    [Header("시네마틱 애니메이션 설정")]
    public CanvasGroup screenFadeCanvasGroup; 
    public float fadeDuration = 0.4f; 
    public float blackHoldDuration = 1.0f;
    public float hudSlideDuration = 0.4f;

    private bool isCinematicMode;

    public bool IsCinematicMode
    {
        get { return isCinematicMode; }
    }

    [Header("Option Panel Inner Setting")]
    public GameObject optionInnerContent;     

    [Header("HUD Elements Categorized by Direction")]
    public RectTransform[] leftHUDElements;
    public RectTransform[] rightHUDElements;
    public RectTransform[] topHUDElements;
    public RectTransform[] bottomHUDElements;

    [Header("메뉴 연출 중 즉시 숨길 HUD")]
    [SerializeField]
    private GameObject[] hideHUDElements;

    private Dictionary<GameObject, bool> instantHUDOriginalStates = new Dictionary<GameObject, bool>();

    private Coroutine restoreHUDCoroutine;

    private bool HUDElementsHidden;

    private Dictionary<RectTransform, Vector2> originalHUDPositions = new Dictionary<RectTransform, Vector2>();
    private bool isTransitioning = false; 

    private RectTransform quitPanelRect;
    private Vector2 originalQuitPanelPos;

    public bool isBattleMode { get; set; } = false;

    [Header("메시지 연출 설정")]
    [Tooltip("메시지가 위로 올라가며 사라지는 전체 시간")]
    [SerializeField] private float messageDisplayTime = 1.2f;
    [Tooltip("메시지가 위로 이동하는 거리")]
    [SerializeField] private float messageRiseDistance = 100f;
    [Tooltip("처음 나타날 때 살짝 커지는 시간")]
    [SerializeField] private float messagePopDuration = 0.12f;
    [SerializeField] private CanvasGroup messageCanvasGroup;
    [SerializeField] private RectTransform messageRect;

    private Vector2 originalMessagePosition;
    private Sequence messageSequence;
    private bool isMessageInitialized;

    private Stack<GameObject> UIStack = new Stack<GameObject>();

    private bool isExternalModalOpen;
    public bool IsExternalModalOpen
    {
        get { return isExternalModalOpen; }
    }
    public bool IsUIOpen => UIStack.Count > 0 || isExternalModalOpen;
    public bool IsInTargetingMode { get; set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        FindLocalPlayerStat();
        StoreOriginalHUDPositions();

        if (QuitPanel != null)
        {
            quitPanelRect = QuitPanel.GetComponent<RectTransform>();
            if (quitPanelRect != null)
            {
                originalQuitPanelPos = quitPanelRect.anchoredPosition;
            }
        }

        if (UI_QuestPanel == null && questPanel != null)
        {
            UI_QuestPanel = questPanel.GetComponent<UI_QuestPanel>();
        }

        if (partyFormationPanel != null) partyFormationPanel.SetActive(false);

        if (optionPanel != null) optionPanel.SetActive(false);

        if (blurPanel != null) blurPanel.SetActive(false);

        if (screenFadeCanvasGroup != null)
        {
            screenFadeCanvasGroup.alpha = 0f;
            screenFadeCanvasGroup.gameObject.SetActive(false);
        }

        InitializeMessage();
    }

    private void Update()
    {
        if (isCinematicMode) return;

        if (isExternalModalOpen) return;

        if (isTransitioning) return;

        if (optionPanel != null && optionPanel.activeSelf)
        {
            UpdatePhoneClock();
        }

        if (!IsUIOpen && (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.LeftAlt)))
        {
            UpdateCursorState();
        }

        if (!IsUIOpen)
        {
            if (Input.GetKeyDown(KeyCode.J)) ToggleQuestPanel();
            if (Input.GetKeyDown(KeyCode.C)) ToggleCharacterInfoPanel();
            if (Input.GetKeyDown(KeyCode.M)) ToggleLocalMapPanel();
            if (Input.GetKeyDown(KeyCode.I)) ToggleInventoryPanel();
            if (Input.GetKeyDown(KeyCode.L)) TogglePartyFormationPanel();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.J) && questPanel.activeSelf) ToggleQuestPanel();
            if (Input.GetKeyDown(KeyCode.C) && statusPanel.activeSelf) ToggleCharacterInfoPanel();
            if (Input.GetKeyDown(KeyCode.M) && localMapPanel != null && localMapPanel.activeSelf) ToggleLocalMapPanel();
            if (Input.GetKeyDown(KeyCode.I) && inventoryPanel.activeSelf) ToggleInventoryPanel();
            if (Input.GetKeyDown(KeyCode.L) && partyFormationPanel != null && partyFormationPanel.activeSelf) TogglePartyFormationPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIStack.Count > 0)
            {
                GameObject topUI = UIStack.Peek();

                if (topUI == localMapPanel)
                {
                    LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
                    if (controller != null) controller.CloseLocalMap();
                }

                if (UIStack.Count > 0 && UIStack.Peek() == topUI)
                {
                    CloseTopUI();
                }
            }
            else
            {
                if (optionPanel != null)
                {
                    OpenOptionPanel();
                }
            }
        }

        if (activeCharacterStat != null && UI_StatusBar != null && UI_StatusBar.isActiveAndEnabled)
        {
            UI_StatusBar.UpdateStatus(activeCharacterStat);
        }
    }

    #region HUD Sliding System
    private void StoreOriginalHUDPositions()
    {
        originalHUDPositions.Clear();
        RegisterHUDArray(leftHUDElements);
        RegisterHUDArray(rightHUDElements);
        RegisterHUDArray(topHUDElements);
        RegisterHUDArray(bottomHUDElements);
    }

    private void RegisterHUDArray(RectTransform[] elements)
    {
        if (elements == null) return;
        foreach (var rect in elements)
        {
            if (rect != null && !originalHUDPositions.ContainsKey(rect))
            {
                originalHUDPositions.Add(rect, rect.anchoredPosition);
            }
        }
    }

    private void HideInstantHUDElements()
    {
        if (restoreHUDCoroutine != null)
        {
            StopCoroutine(restoreHUDCoroutine);
            restoreHUDCoroutine = null;
        }

        if (HUDElementsHidden)
        {
            return;
        }

        instantHUDOriginalStates.Clear();

        if (hideHUDElements == null)
        {
            HUDElementsHidden = true;
            return;
        }

        foreach (GameObject hudObject in hideHUDElements)
        {
            if (hudObject == null) continue;

            if (instantHUDOriginalStates.ContainsKey(hudObject))
            {
                continue;
            }

            if (hudObject == messagePanel)
            {
                HideMessage();
            }

            instantHUDOriginalStates.Add(hudObject, hudObject.activeSelf);

            hudObject.SetActive(false);
        }

        HUDElementsHidden = true;
    }

    private void RequestRestoreInstantHUDElements(float delay)
    {
        if (restoreHUDCoroutine != null)
        {
            StopCoroutine(restoreHUDCoroutine);
        }

        restoreHUDCoroutine = StartCoroutine(RestoreInstantHUDElementsRoutine(delay));
    }

    private IEnumerator RestoreInstantHUDElementsRoutine(float delay)
    {
        if (!HUDElementsHidden)
        {
            restoreHUDCoroutine = null;
            yield break;
        }

        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        foreach (var pair in instantHUDOriginalStates)
        {
            GameObject hudObject = pair.Key;
            bool wasActive = pair.Value;

            if (hudObject == null) continue;

            hudObject.SetActive(wasActive);
        }

        if (activeCharacterStat != null && UI_StatusBar != null && UI_StatusBar.isActiveAndEnabled)
        {
            UI_StatusBar.UpdateStatus(activeCharacterStat);
        }

        instantHUDOriginalStates.Clear();

        HUDElementsHidden = false;
        restoreHUDCoroutine = null;
    }

    public void AnimateHUD(bool show, float duration)
    {
        if (show)
        {
            RequestRestoreInstantHUDElements(duration);
        }
        else
        {
            HideInstantHUDElements();
        }

        foreach (var pair in originalHUDPositions)
        {
            RectTransform rect = pair.Key;
            Vector2 originPos = pair.Value;

            if (rect == null) continue;
            rect.DOKill();

            if (show)
            {
                rect.DOAnchorPos(originPos, duration).SetEase(Ease.OutCubic).SetUpdate(true);
            }
            else
            {
                Vector2 hidePos = originPos;
                if (System.Array.IndexOf(leftHUDElements, rect) >= 0) hidePos.x -= 800f;
                else if (System.Array.IndexOf(rightHUDElements, rect) >= 0) hidePos.x += 800f;
                else if (System.Array.IndexOf(topHUDElements, rect) >= 0) hidePos.y += 500f;
                else if (System.Array.IndexOf(bottomHUDElements, rect) >= 0) hidePos.y -= 500f;

                rect.DOAnchorPos(hidePos, duration).SetEase(Ease.InCubic).SetUpdate(true);
            }
        }
    }

    public void EnterCinematicMode()
    {
        if (isCinematicMode) return;

        isCinematicMode = true;
        HideMessage();

        AnimateHUD(false, hudSlideDuration);
    }

    public void ExitCinematicMode()
    {
        if (!isCinematicMode) return;

        isCinematicMode = false;

        if (UIStack.Count == 0)
        {
            AnimateHUD(true, hudSlideDuration);
        }
    }
    #endregion

    #region Option Panel Specific Logic (ESC)
    public void OpenOptionPanel()
    {
        if (optionPanel.activeSelf) return;

        AnimateHUD(false, hudSlideDuration);

        optionPanel.SetActive(true);
        optionPanel.transform.SetAsLastSibling();
        UIStack.Push(optionPanel);
        Time.timeScale = 0f;
        UpdateCursorState();

        if (optionInnerContent != null) optionInnerContent.SetActive(false);

        optionPanel.transform.DOKill();
        optionPanel.transform.localScale = Vector3.zero;

        bool innerActivated = false;
        optionPanel.transform.DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnUpdate(() =>
            {
                if (!innerActivated && optionPanel.transform.localScale.x >= 0.8f)
                {
                    innerActivated = true;
                    if (optionInnerContent != null) optionInnerContent.SetActive(true);
                }
            });
    }

    private void CloseOptionPanel()
    {
        if (!optionPanel.activeSelf) return;

        optionPanel.transform.DOKill();
        optionPanel.SetActive(false);

        if (UIStack.Count > 0 && UIStack.Peek() == optionPanel) UIStack.Pop();

        CheckTimeScale();
        UpdateCursorState();

        if (UIStack.Count == 0)
        {
            AnimateHUD(true, hudSlideDuration);
        }
    }
    #endregion

    #region Quit Panel Specific Logic
    private void OpenQuitPanel()
    {
        if (QuitPanel == null || QuitPanel.activeSelf) return;

        // 배경 블러 패널 처리 유지
        if (blurPanel != null)
        {
            blurPanel.SetActive(true);
            blurPanel.transform.SetAsLastSibling();
        }

        QuitPanel.SetActive(true);
        QuitPanel.transform.SetAsLastSibling();
        UIStack.Push(QuitPanel);
        Time.timeScale = 0f;
        UpdateCursorState();

        if (quitPanelRect != null)
        {
            quitPanelRect.DOKill();
            quitPanelRect.anchoredPosition = new Vector2(originalQuitPanelPos.x, originalQuitPanelPos.y - 1000f);
            quitPanelRect.DOAnchorPos(originalQuitPanelPos, 0.4f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }
    }

    public void CloseQuitPanel()
    {
        if (QuitPanel == null || !QuitPanel.activeSelf) return;

        if (blurPanel != null) blurPanel.SetActive(false);

        if (UIStack.Count > 0 && UIStack.Peek() == QuitPanel) UIStack.Pop();

        CheckTimeScale();
        UpdateCursorState();

        if (quitPanelRect != null)
        {
            quitPanelRect.DOKill();
            quitPanelRect.DOAnchorPos(new Vector2(originalQuitPanelPos.x, originalQuitPanelPos.y - 1000f), 0.3f)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    QuitPanel.SetActive(false);
                });
        }
        else
        {
            QuitPanel.SetActive(false);
        }
    }
    #endregion

    #region Coroutine Based Cinematic Transitions (Sub-Panels)
    private IEnumerator OpenUITransitionRoutine(GameObject panel, System.Action preOpenAction = null)
    {
        isTransitioning = true;

        Time.timeScale = 0f;
        

        AnimateHUD(false, hudSlideDuration);
        yield return new WaitForSecondsRealtime(hudSlideDuration);

        if (screenFadeCanvasGroup != null)
        {
            screenFadeCanvasGroup.gameObject.SetActive(true);
            screenFadeCanvasGroup.transform.SetAsLastSibling(); // 최상단 배치
            yield return screenFadeCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(blackHoldDuration);

        preOpenAction?.Invoke();
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        if (screenFadeCanvasGroup != null)
        {
            screenFadeCanvasGroup.transform.SetAsLastSibling();
        }

        UIStack.Push(panel);
        UpdateCursorState();

        if (screenFadeCanvasGroup != null)
        {
            yield return screenFadeCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).WaitForCompletion();
            screenFadeCanvasGroup.gameObject.SetActive(false);
        }

        isTransitioning = false;
    }

    private IEnumerator CloseUITransitionRoutine(GameObject panel, bool isTopUI, System.Action postCloseAction = null)
    {
        isTransitioning = true;

        if (screenFadeCanvasGroup != null)
        {
            screenFadeCanvasGroup.gameObject.SetActive(true);
            screenFadeCanvasGroup.transform.SetAsLastSibling();
            yield return screenFadeCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();
        }

        panel.transform.DOKill();
        panel.SetActive(false);
        postCloseAction?.Invoke();

        if (isTopUI)
        {
            if (UIStack.Count > 0 && UIStack.Peek() == panel) UIStack.Pop();
        }
        else
        {
            RemoveFromStack(panel);
        }

        CheckTimeScale();
        UpdateCursorState();

        if (UIStack.Count == 0)
        {
            AnimateHUD(true, hudSlideDuration);
        }

        if (screenFadeCanvasGroup != null)
        {
            yield return screenFadeCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).WaitForCompletion();
            screenFadeCanvasGroup.gameObject.SetActive(false);
        }

        isTransitioning = false;
    }

    private void RemoveFromStack(GameObject panel)
    {
        if (UIStack.Contains(panel))
        {
            Stack<GameObject> tempStack = new Stack<GameObject>();
            while (UIStack.Count > 0)
            {
                GameObject top = UIStack.Pop();
                if (top != panel) tempStack.Push(top);
            }
            while (tempStack.Count > 0)
            {
                UIStack.Push(tempStack.Pop());
            }
        }
    }
    #endregion

    #region Core Open / Close Interfaces
    public void OpenUI(GameObject panel)
    {
        if (panel == null || panel.activeSelf) return;

        if (panel == optionPanel)
        {
            OpenOptionPanel();
        }
        else if (panel == QuitPanel)
        {
            OpenQuitPanel();
        }
        else
        {
            StartCoroutine(OpenUITransitionRoutine(panel));
        }
    }

    public void CloseTopUI()
    {
        if (UIStack.Count > 0)
        {
            GameObject topUI = UIStack.Peek();
            if (topUI == optionPanel)
            {
                CloseOptionPanel();
            }
            else if (topUI == QuitPanel)
            {
                CloseQuitPanel();
            }
            else
            {
                System.Action postAction = null;
                if (topUI == partyFormationPanel)
                {
                    UI_PartyFormation formationScript = partyFormationPanel.GetComponent<UI_PartyFormation>();
                    if (formationScript != null)
                    {
                        postAction = () => formationScript.SaveAndClose();
                    }
                }

                StartCoroutine(CloseUITransitionRoutine(topUI, true, postAction));
            }
        }
    }

    public void CloseSpecificUI(GameObject panel)
    {
        if (panel == null || !panel.activeSelf) return;

        if (panel == optionPanel)
        {
            CloseOptionPanel();
        }
        else if (panel == QuitPanel)
        {
            CloseQuitPanel();
        }
        else
        {
            StartCoroutine(CloseUITransitionRoutine(panel, false));
        }
    }
    #endregion

    #region Toggle Triggers (Modified)
    public void ToggleQuestPanel()
    {
        if (questPanel.activeSelf) CloseSpecificUI(questPanel);
        else if (!IsUIOpen) OpenUI(questPanel);
    }

    public void ToggleInventoryPanel()
    {
        if (inventoryPanel.activeSelf) CloseSpecificUI(inventoryPanel);
        else if (!IsUIOpen)
        {
            StartCoroutine(OpenUITransitionRoutine(inventoryPanel, () => {
                if (UI_Inventory != null) UI_Inventory.RefreshUI();
            }));
        }
    }

    public void ToggleCharacterInfoPanel()
    {
        if (statusPanel == null) return;

        if (statusPanel.activeSelf)
        {
            CloseSpecificUI(statusPanel);
            return;
        }

        if (IsCombatRestrictedUIBlocked()) return;

        if (!IsUIOpen)
        {
            StartCoroutine(OpenUITransitionRoutine(statusPanel, () => {
                UpdatePlayerStatus();
            }));
        }
    }

    public void ToggleLocalMapPanel()
    {
        if (localMapPanel == null) return;

        if (localMapPanel.activeSelf)
        {
            StartCoroutine(CloseUITransitionRoutine(localMapPanel, false, () => {
                LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
                if (controller != null) controller.CloseLocalMap();
            }));
        }

        if (IsCombatRestrictedUIBlocked()) return;
        
        if (!IsUIOpen)
        {
            StartCoroutine(OpenUITransitionRoutine(localMapPanel, () => {
                LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
                if (controller != null) controller.OpenLocalMap();
            }));
        }
    }

    public void TogglePartyFormationPanel()
    {
        if (partyFormationPanel == null) return;

        if (partyFormationPanel.activeSelf)
        {
            StartCoroutine(CloseUITransitionRoutine(partyFormationPanel, false, () => {
                UI_PartyFormation formationScript = partyFormationPanel.GetComponent<UI_PartyFormation>();
                if (formationScript != null) formationScript.SaveAndClose();
            }));

            return;
        }

        if (IsCombatRestrictedUIBlocked()) return;
        
        if (!IsUIOpen)
        {
            StartCoroutine(OpenUITransitionRoutine(partyFormationPanel, () => {
                UI_PartyFormation formationScript = partyFormationPanel.GetComponent<UI_PartyFormation>();
                if (formationScript != null) formationScript.OpenFormationWindow();
            }));
        }
    }
    #endregion

    #region Option Menu Buttons
    public void OpenInventoryFromOption() 
    { 
        if (inventoryPanel != null && !inventoryPanel.activeSelf) 
        { 
            UI_Inventory.RefreshUI(); OpenUI(inventoryPanel); 
        } 
    }
    public void OpenLocalMapFromOption() 
    {
        if (IsCombatRestrictedUIBlocked()) return;

        if (localMapPanel != null && !localMapPanel.activeSelf) 
        { 
            OpenUI(localMapPanel); 
            LocalMapController controller = localMapPanel.GetComponent<LocalMapController>();
            if (controller != null) controller.OpenLocalMap(); 
        } 
    }
    public void OpenCharacterInfoFromOption() 
    { 
        if (IsCombatRestrictedUIBlocked()) return;

        if (statusPanel != null && !statusPanel.activeSelf) 
        { 
            UpdatePlayerStatus(); 
            OpenUI(statusPanel); 
        } 
    }
    public void OpenQuestFromOption() 
    { 
        if (questPanel != null && !questPanel.activeSelf) OpenUI(questPanel); 
    }
    public void OpenPartyFormationFromOption() 
    {
        if (IsCombatRestrictedUIBlocked()) return;

        if (partyFormationPanel != null && !partyFormationPanel.activeSelf) 
        { 
            OpenUI(partyFormationPanel);
            UI_PartyFormation formationScript = partyFormationPanel.GetComponent<UI_PartyFormation>(); 
            if (formationScript != null) formationScript.OpenFormationWindow(); 
        } 
    }

    public void OpenQuitPanelFromOption()
    {
        if (QuitPanel.activeSelf)
        {
            CloseQuitPanel();
        }
        else
        {
            OpenQuitPanel();
        }
    }
    #endregion

    #region Helper & Legacy Methods
    private void UpdatePhoneClock() 
    { 
        if (phoneTimeText != null) phoneTimeText.text = System.DateTime.Now.ToString("HH:mm"); 
    }
    public void ToggleMainHUD(bool show) { }

    public void HideInteractionPrompt()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    private void FindLocalPlayerStat()
    {
        Character_Stat[] allStats = FindObjectsOfType<Character_Stat>();
        foreach (var stat in allStats)
        {
            if (stat.gameObject.activeInHierarchy && stat.CompareTag("Player"))
            {
                activeCharacterStat = stat;
                break;
            }
        }
    }

    public void UpdatePlayerStatus(Character_Stat stat = null)
    {
        if (stat != null) activeCharacterStat = stat;
        if (activeCharacterStat == null) FindLocalPlayerStat();
        if (activeCharacterStat != null && UI_StatusBar != null && UI_StatusBar.isActiveAndEnabled) UI_StatusBar.UpdateStatus(activeCharacterStat);
    }

    public void SetMinimapVisible(bool visible)
    {
        if (minimapPanel == null) return;

        minimapPanel.SetActive(visible);

        if (visible && BattleManager.Instance != null)
        {
            GameObject activeCharacter = BattleManager.Instance.GetActiveCharacter();

            if (activeCharacter == null) return;

            MiniMapController minimapController = minimapPanel.GetComponentInParent<MiniMapController>(true);

            if (minimapController != null)
            {
                minimapController.SetTarget(activeCharacter.transform);
            }
        }
    }

    //보스맵 퇴장 시
    public void SetExternalModalOpen(bool isOpen)
    {
        isExternalModalOpen = isOpen;

        if (isOpen)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = UIStack.Count > 0 ? 0f : 1f;
        }

        UpdateCursorState();
    }

    public void ShowMessage(string msg)
    {
        if (messagePanel == null || messageText == null)
        {
            return;
        }

        if (HUDElementsHidden)
        {
            return;
        }

        if (!isMessageInitialized)
        {
            InitializeMessage();
        }

        if (messageRect == null ||
            messageCanvasGroup == null)
        {
            return;
        }

        if (messageSequence != null)
        {
            messageSequence.Kill();
            messageSequence = null;
        }

        messageRect.DOKill();
        messageCanvasGroup.DOKill();

        messagePanel.SetActive(true);
        messagePanel.transform.SetAsLastSibling();

        messageText.text = msg;
        messageRect.anchoredPosition = originalMessagePosition;
        messageRect.localScale = Vector3.one * 0.85f;

        messageCanvasGroup.alpha = 1f;
        messageCanvasGroup.interactable = false;
        messageCanvasGroup.blocksRaycasts = false;

        float animationDuration = Mathf.Max(messageDisplayTime, 0.1f);

        messageSequence = DOTween.Sequence();
        messageSequence.SetUpdate(true);
        messageSequence.Insert(0f, messageRect.DOScale(Vector3.one, messagePopDuration).SetEase(Ease.OutBack));
        messageSequence.Insert(0f, messageRect.DOAnchorPos(originalMessagePosition + Vector2.up * messageRiseDistance, animationDuration).SetEase(Ease.OutCubic));
        messageSequence.Insert(0f, messageCanvasGroup.DOFade(0f, animationDuration).SetEase(Ease.InQuad));

        messageSequence.OnComplete(() =>
        {
            messagePanel.SetActive(false);
            messageRect.anchoredPosition = originalMessagePosition;
            messageRect.localScale = Vector3.one;
            messageCanvasGroup.alpha = 0f;
            messageSequence = null;
        });
    }

    public void HideMessage()
    {
        if (messageSequence != null)
        {
            messageSequence.Kill();
            messageSequence = null;
        }

        if (messageRect != null)
        {
            messageRect.DOKill();
            messageRect.anchoredPosition = originalMessagePosition;
            messageRect.localScale = Vector3.one;
        }

        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.DOKill();
            messageCanvasGroup.alpha = 0f;
        }

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    private void InitializeMessage()
    {
        if (messagePanel == null) return;

        if (messageRect == null) messageRect = messagePanel.GetComponent<RectTransform>();

        if (messageCanvasGroup == null) messageCanvasGroup = messagePanel.GetComponent<CanvasGroup>();

        if (messageRect != null) originalMessagePosition = messageRect.anchoredPosition;

        messageCanvasGroup.alpha = 0f;
        messageCanvasGroup.interactable = false;
        messageCanvasGroup.blocksRaycasts = false;

        messagePanel.SetActive(false);

        isMessageInitialized = true;
    }

    private void CheckTimeScale() 
    { 
        Time.timeScale = (UIStack.Count == 0) ? 1f : 0f; 
    }

    public void UpdateCursorState()
    {
        if (IsUIOpen || IsInTargetingMode || isBattleMode)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            return;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private bool IsCombatRestrictedUIBlocked()
    {
        if (BattleManager.Instance == null) return false;

        GameObject activeCharacter = BattleManager.Instance.GetActiveCharacter();

        if (activeCharacter == null) return false;

        Player_Equipment equipment = activeCharacter.GetComponent<Player_Equipment>();

        if (equipment == null || !equipment.IsInCombat) return false;

        ShowMessage("전투 중에는 열 수 없는 패널입니다.");

        return true;
    }

    public void SetBattleMode(bool isBattle) 
    { 
        isBattleMode = isBattle; UpdateCursorState(); 
    }
    public void ShowCursor() 
    { 
        Cursor.visible = true; Cursor.lockState = CursorLockMode.None; 
    }
    public void HideCursor() 
    { 
        UpdateCursorState(); 
    }
    public void OnClickGameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}