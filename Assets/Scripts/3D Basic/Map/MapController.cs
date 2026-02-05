using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class MapLocationData
{
    public int id;
    public string name;
    public string description;
    public int levelLimit;
    public string sceneName;
    public string imageName;
}

public class MapController : MonoBehaviour
{
    [Header("--- UI 연결: 메인 ---")]
    [SerializeField] private ScrollRect mapScrollRect;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private Button closeButton;

    [Header("--- UI 연결: 장소 정보 패널 ---")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private RectTransform infoPanelRect;
    [SerializeField] private Text infoName;
    [SerializeField] private Text infoDesc;
    [SerializeField] private Text infoLevel;
    [SerializeField] private Image infoImage;
    [SerializeField] private Button btnEnter;
    [SerializeField] private Button btnCancel;

    [Header("--- 줌/포커스 설정 ---")]
    [SerializeField] private float zoomScale = 1.5f;
    [SerializeField] private float animDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutExpo;

    [Header("플레이어 위치 표시")]
    [SerializeField] private GameObject playerIcon;
    [SerializeField] private Vector3 iconOffset = new Vector3(0, 0, 0);

    [Header("GPS 핀 시스템")]
    [SerializeField] private GameObject pinPrefab;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private RectTransform viewportRect;
    [SerializeField] private Transform indicatorParent;
    [SerializeField] private Vector3 pinOffset = new Vector3(0, 60, 0);

    private class MapTarget
    {
        public Transform buttonTrans;
        public GameObject pinObj;
        public GameObject indicatorObj;
        public RectTransform indicatorRect;
    }
    private List<MapTarget> managedTargets = new List<MapTarget>();

    [Header("맵 버튼 설정")]
    [SerializeField] private List<MapButtonEntry> mapEntries;

    private Dictionary<int, MapLocationData> mapDataDict = new Dictionary<int, MapLocationData>();
    private MapLocationData currentTargetData;

    private bool isLobbyMode = false;
    private bool isFocused = false;

    [System.Serializable]
    public struct MapButtonEntry
    {
        public int locationID;
        public string sceneName;
        public Button buttonObj;
        public string targetSpawnName;
    }

    private void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Main Menu" || currentScene == "Lobby") isLobbyMode = true;

        LoadMapData();

        if (infoPanel) infoPanel.SetActive(false);
    }

    private void Start()
    {
        if (closeButton) closeButton.onClick.AddListener(CloseMapPanel);
        if (btnEnter) btnEnter.onClick.AddListener(OnEnterLocation);
        if (btnCancel) btnCancel.onClick.AddListener(() => ResetMapAndCloseInfo(false));

        UpdateCloseButtonState();
    }

    private void OnEnable()
    {
        InitializeMap();

        UpdateCloseButtonState();
    }

    private void Update()
    {
        UpdateIndicators();
    }

    private void UpdateCloseButtonState()
    {
        if (closeButton != null)
        {
            // 로비 모드면 닫기 버튼 숨김 (전역 뒤로가기 사용), 아니면 보임
            closeButton.gameObject.SetActive(!isLobbyMode);
        }
    }

    void LoadMapData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("Data/CSV/Map");
        if (csvData == null) return;

        string[] lines = csvData.text.Replace("\r\n", "\n").Split('\n');
        string pattern = @",(?=(?:[^""]*""[^""]*"")*[^""]*$)";

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = Regex.Split(lines[i], pattern);

            if (cols.Length < 6) continue;

            MapLocationData data = new MapLocationData();
            try
            {
                data.id = int.Parse(cols[0]);
                data.name = cols[1];
                data.description = cols[2].Replace("\"", "");
                data.levelLimit = int.Parse(cols[3]);
                data.sceneName = cols[4];
                data.imageName = cols[5].Trim();

                if (!mapDataDict.ContainsKey(data.id))
                    mapDataDict.Add(data.id, data);
            }
            catch { Debug.LogWarning("CSV 파싱 에러: " + i + "번째 줄"); }
        }
    }

    private void InitializeMap()
    {
        if (!isLobbyMode)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        ResetMapAndCloseInfo(true);

        UpdatePlayerIconPosition();
        CreatePinsAndIndicators();

        string currentScene = SceneManager.GetActiveScene().name;
        Button startButton = null;
        foreach (var entry in mapEntries)
        {
            if (entry.sceneName == currentScene)
            {
                startButton = entry.buttonObj;
                break;
            }
        }
        if (startButton == null && mapEntries.Count > 0) startButton = mapEntries[0].buttonObj;
        if (startButton != null) EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    void CreatePinsAndIndicators()
    {
        foreach (var target in managedTargets)
        {
            if (target.pinObj) Destroy(target.pinObj);
            if (target.indicatorObj) Destroy(target.indicatorObj);
        }
        managedTargets.Clear();

        if (pinPrefab == null || indicatorPrefab == null) return;

        foreach (var entry in mapEntries)
        {
            if (entry.buttonObj == null) continue;

            GameObject newPin = Instantiate(pinPrefab, entry.buttonObj.transform);
            newPin.transform.localPosition = pinOffset;

            UI_MapPin pinScript = newPin.GetComponent<UI_MapPin>();
            if (pinScript) pinScript.Setup();

            UI_MapBuilding buildingScript = entry.buttonObj.GetComponent<UI_MapBuilding>();
            if (buildingScript) buildingScript.linkedPin = pinScript;

            GameObject newIndicator = Instantiate(indicatorPrefab, indicatorParent);
            newIndicator.SetActive(false);

            MapTarget newTarget = new MapTarget();
            newTarget.buttonTrans = entry.buttonObj.transform;
            newTarget.pinObj = newPin;
            newTarget.indicatorObj = newIndicator;
            newTarget.indicatorRect = newIndicator.GetComponent<RectTransform>();

            managedTargets.Add(newTarget);
        }
    }

    void UpdateIndicators()
    {
        if (viewportRect == null) return;
        Vector3[] corners = new Vector3[4];
        viewportRect.GetWorldCorners(corners);
        Rect viewRect = new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);

        foreach (var target in managedTargets)
        {
            if (target.buttonTrans == null) continue;
            Vector3 btnPos = target.buttonTrans.position;
            bool isVisible = viewRect.Contains(btnPos);

            if (isVisible)
            {
                if (target.pinObj) target.pinObj.SetActive(true);
                if (target.indicatorObj) target.indicatorObj.SetActive(false);
            }
            else
            {
                if (target.pinObj) target.pinObj.SetActive(false);
                if (target.indicatorObj)
                {
                    target.indicatorObj.SetActive(true);
                    Vector3 indicatorPos = btnPos;
                    indicatorPos.x = Mathf.Clamp(indicatorPos.x, viewRect.x + 30, viewRect.xMax - 30);
                    indicatorPos.y = Mathf.Clamp(indicatorPos.y, viewRect.y + 30, viewRect.yMax - 30);
                    target.indicatorObj.transform.position = indicatorPos;
                }
            }
        }
    }

    public void UpdatePlayerIconPosition()
    {
        if (playerIcon == null) return;
        string currentScene = SceneManager.GetActiveScene().name;
        bool found = false;

        foreach (var entry in mapEntries)
        {
            if (entry.sceneName == currentScene && entry.buttonObj != null)
            {
                playerIcon.SetActive(true);
                playerIcon.transform.position = entry.buttonObj.transform.position;
                playerIcon.transform.localPosition += iconOffset;
                playerIcon.transform.SetAsLastSibling();
                found = true;
                break;
            }
        }
        if (!found) playerIcon.SetActive(false);
    }

    public void OnClickMapButton(int index)
    {
        if (isFocused) return;
        if (index < 0 || index >= mapEntries.Count) return;

        MapButtonEntry entry = mapEntries[index];

        if (mapDataDict.ContainsKey(entry.locationID))
        {
            currentTargetData = mapDataDict[entry.locationID];

            // 1. 버튼 위치 판단 (Pivot Center(0.5, 0.5) 기준)
            // x > 0 이면 중앙보다 오른쪽, x < 0 이면 중앙보다 왼쪽
            bool isRightSide = entry.buttonObj.transform.localPosition.x > 0;

            // 2. 정보 패널 위치 갱신
            UpdateInfoPanel(currentTargetData, isRightSide);

            // 3. 분할 화면 포커싱 시작
            FocusMap(entry.buttonObj.GetComponent<RectTransform>(), isRightSide);
        }
        else
        {
            Debug.LogError($"ID {entry.locationID} 데이터 없음.");
        }
    }

    void UpdateInfoPanel(MapLocationData data, bool isButtonOnRight)
    {
        if (isLobbyMode && LobbyManager.instance != null && LobbyManager.instance.globalBackButton != null)
        {
            LobbyManager.instance.globalBackButton.SetActive(false);
        }

        if (infoPanel)
        {
            infoPanel.SetActive(true);

            if (infoPanelRect != null)
            {
                infoPanelRect.offsetMin = Vector2.zero;
                infoPanelRect.offsetMax = Vector2.zero;

                if (isButtonOnRight)
                {
                    // 버튼 오른쪽 -> 패널 왼쪽
                    infoPanelRect.anchorMin = new Vector2(0, 0);
                    infoPanelRect.anchorMax = new Vector2(0.5f, 1);
                }
                else
                {
                    // 버튼 왼쪽 -> 패널 오른쪽
                    infoPanelRect.anchorMin = new Vector2(0.5f, 0);
                    infoPanelRect.anchorMax = new Vector2(1, 1);
                }

                infoPanelRect.anchoredPosition = Vector2.zero;
                infoPanelRect.sizeDelta = Vector2.zero;
            }
        }

        if (infoName) infoName.text = data.name;
        if (infoDesc) infoDesc.text = data.description;
        if (infoLevel) infoLevel.text = $"권장 레벨: {data.levelLimit}";

        Sprite img = Resources.Load<Sprite>($"Image/Scene/MapPreview/{data.imageName}");
        if (img != null && infoImage != null)
        {
            infoImage.sprite = img;
        }
    }

    // [수정] 피벗(0.5, 0.5) 기준, 분할 화면 포커싱
    void FocusMap(RectTransform targetBtn, bool isButtonOnRight)
    {
        isFocused = true;
        mapScrollRect.enabled = false;

        float viewportW = mapScrollRect.viewport.rect.width;
        Vector2 btnPos = targetBtn.anchoredPosition;

        float targetScreenX = isButtonOnRight ? (viewportW * 0.25f) : (viewportW * -0.25f);
        float targetScreenY = 0f;

        float targetContentX = targetScreenX - (btnPos.x * zoomScale);
        float targetContentY = targetScreenY - (btnPos.y * zoomScale);

        float scaledHalfW = (contentRect.rect.width * zoomScale) / 2f;
        float scaledHalfH = (contentRect.rect.height * zoomScale) / 2f;
        float viewHalfW = mapScrollRect.viewport.rect.width / 2f;
        float viewHalfH = mapScrollRect.viewport.rect.height / 2f;

        float limitX = Mathf.Max(0, scaledHalfW - viewHalfW);
        float limitY = Mathf.Max(0, scaledHalfH - viewHalfH);

        Vector2 endPos = new Vector2(
            Mathf.Clamp(targetContentX, -limitX, limitX),
            Mathf.Clamp(targetContentY, -limitY, limitY)
        );

        contentRect.DOKill();

        contentRect.DOAnchorPos(endPos, animDuration)
            .SetEase(moveEase)
            .SetUpdate(true);

        contentRect.DOScale(zoomScale, animDuration)
            .SetEase(moveEase)
            .SetUpdate(true);
    }

    public void ResetMapAndCloseInfo(bool immediate)
    {
        isFocused = false;
        if (infoPanel) infoPanel.SetActive(false);
        mapScrollRect.enabled = true;

        if (isLobbyMode && LobbyManager.instance != null && LobbyManager.instance.globalBackButton != null)
        {
            LobbyManager.instance.globalBackButton.SetActive(true);
        }

        contentRect.DOKill();

        if (immediate)
        {
            contentRect.localScale = Vector3.one;
            contentRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            contentRect.DOAnchorPos(Vector2.zero, animDuration)
                .SetEase(moveEase)
                .SetUpdate(true);

            contentRect.DOScale(1f, animDuration)
                .SetEase(moveEase)
                .SetUpdate(true);
        }
    }

    void OnEnterLocation()
    {
        if (currentTargetData != null)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(currentTargetData.sceneName);
        }
    }

    public void CloseMapPanel()
    {
        Time.timeScale = 1f;
        ResetMapAndCloseInfo(true);
        if (isLobbyMode)
        {
            // [로비 모드] LobbyManager에게 뒤로가기 위임
            if (LobbyManager.instance != null)
                LobbyManager.instance.OnClickBack();
            else
                gameObject.SetActive(false);
        }
        else
        {
            // [3D 인게임 모드] UI_Manager를 통해 닫기 (DOTween 스택 관리 포함)
            if (UI_Manager.instance != null)
                UI_Manager.instance.CloseSpecificUI(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}