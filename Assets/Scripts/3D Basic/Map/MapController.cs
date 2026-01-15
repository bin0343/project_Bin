using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Main Menu" || currentScene == "Lobby") isLobbyMode = true;

        if (closeButton) closeButton.onClick.AddListener(CloseMapPanel);
        if (btnEnter) btnEnter.onClick.AddListener(OnEnterLocation);
        if (btnCancel) btnCancel.onClick.AddListener(() => ResetMapAndCloseInfo(false));

        // [중요] Pivot 강제 변경 코드 삭제. 
        // 에디터에서 Content Pivot을 (0.5, 0.5)로 설정한 것을 그대로 따릅니다.

        LoadMapData();

        if (infoPanel) infoPanel.SetActive(false);
    }

    private void OnEnable()
    {
        InitializeMap();
    }

    private void Update()
    {
        UpdateIndicators();
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
            StartCoroutine(AnimateFocus(entry.buttonObj.GetComponent<RectTransform>(), isRightSide));
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
    IEnumerator AnimateFocus(RectTransform targetBtn, bool isButtonOnRight)
    {
        isFocused = true;
        mapScrollRect.enabled = false;

        float viewportW = mapScrollRect.viewport.rect.width;

        // 버튼의 로컬 위치 (중앙 기준)
        Vector2 btnPos = targetBtn.anchoredPosition;

        // [목표 화면 좌표 계산] (Pivot Center 기준)
        // 버튼이 오른쪽(isRight) -> 패널 왼쪽 -> 지도는 화면의 오른쪽(0.25지점)에 포커스
        // 버튼이 왼쪽(!isRight) -> 패널 오른쪽 -> 지도는 화면의 왼쪽(-0.25지점)에 포커스
        // * 중앙이 0이므로, 오른쪽 1/4 지점은 width * 0.25, 왼쪽은 width * -0.25
        float targetScreenX = isButtonOnRight ? (viewportW * 0.25f) : (viewportW * -0.25f);
        float targetScreenY = 0f; // Y축은 중앙 유지

        // [Content 목표 위치 계산]
        // 공식: TargetContentPos = TargetScreenPos - (ButtonPos * ZoomScale)
        float targetContentX = targetScreenX - (btnPos.x * zoomScale);
        float targetContentY = targetScreenY - (btnPos.y * zoomScale);

        // [클램핑 (Clamping)] - Pivot Center 기준
        // 지도의 절반 크기 (확대된 상태)
        float scaledHalfW = (contentRect.rect.width * zoomScale) / 2f;
        float scaledHalfH = (contentRect.rect.height * zoomScale) / 2f;

        // 뷰포트의 절반 크기
        float viewHalfW = mapScrollRect.viewport.rect.width / 2f;
        float viewHalfH = mapScrollRect.viewport.rect.height / 2f;

        // 이동 가능한 최대 범위 (절댓값) = (확대된 지도 반쪽 - 뷰포트 반쪽)
        float limitX = Mathf.Max(0, scaledHalfW - viewHalfW);
        float limitY = Mathf.Max(0, scaledHalfH - viewHalfH);

        Vector2 endPos = new Vector2(
            Mathf.Clamp(targetContentX, -limitX, limitX),
            Mathf.Clamp(targetContentY, -limitY, limitY)
        );

        Vector2 startPos = contentRect.anchoredPosition;
        Vector3 startScale = contentRect.localScale;

        float time = 0;
        while (time < animDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0, 1, time / animDuration);

            contentRect.localScale = Vector3.Lerp(startScale, new Vector3(zoomScale, zoomScale, 1), t);
            contentRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        contentRect.localScale = new Vector3(zoomScale, zoomScale, 1);
        contentRect.anchoredPosition = endPos;
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

        if (immediate)
        {
            contentRect.localScale = Vector3.one;
            contentRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            StartCoroutine(AnimateReset());
        }
    }

    IEnumerator AnimateReset()
    {
        Vector2 startPos = contentRect.anchoredPosition;
        Vector3 startScale = contentRect.localScale;

        Vector2 targetPos = Vector2.zero;
        Vector3 targetScale = Vector3.one;

        float time = 0;
        while (time < animDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0, 1, time / animDuration);

            contentRect.localScale = Vector3.Lerp(startScale, targetScale, t);
            contentRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        contentRect.localScale = Vector3.one;
        contentRect.anchoredPosition = Vector2.zero;
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
        if (isLobbyMode && LobbyManager.instance != null) LobbyManager.instance.OnClickBack();
        else gameObject.SetActive(false);
    }
}