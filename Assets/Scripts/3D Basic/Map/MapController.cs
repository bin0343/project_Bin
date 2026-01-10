using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    [Header("UI 요소 연결")]
    [SerializeField] private GameObject confirmationPanel; // 확인 팝업 패널
    [SerializeField] private Text confirmText;             // "XX로 이동하시겠습니까?" 텍스트
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    [SerializeField] private Button closeButton; // (선택) 지도 닫기/뒤로가기 버튼

    [Header("플레이어 위치 표시")]
    [SerializeField] private GameObject playerIcon;
    [SerializeField] private Vector3 iconOffset = new Vector3 (0, 0, 0);

    [Header("GPS 핀 시스템")]
    [SerializeField] private GameObject pinPrefab;       // 핀 프리팹 (지도 안 버튼 위에 생성됨)
    [SerializeField] private GameObject indicatorPrefab; // 화면 밖 말풍선 프리팹 (패널 최상위에 생성됨)
    [SerializeField] private RectTransform viewportRect; // Scroll View의 Viewport (화면 범위 기준)
    [SerializeField] private Transform indicatorParent;  // 말풍선이 생성될 부모 (보통 Panel_Map 자신)
    [SerializeField] private Vector3 pinOffset = new Vector3(0, 60, 0); // 버튼에서 핀이 떨어질 높이

    private class MapTarget
    {
        public Transform buttonTrans;     // 지역 버튼
        public GameObject pinObj;         // 생성된 핀
        public GameObject indicatorObj;   // 생성된 말풍선
        public RectTransform indicatorRect;
    }
    private List<MapTarget> managedTargets = new List<MapTarget>();

    [Header("맵 버튼 설정")]
    [SerializeField] private List<MapButtonEntry> mapEntries;

    private MapButtonEntry currentTargetEntry;
    private GameObject lastSelectedMapButton;

    private bool isLobbyMode = false; // 로비인지 인게임인지 구분

    [System.Serializable]
    public struct MapButtonEntry
    {
        public string locationName; // UI 표시용 (예: 숲, 학교)
        public string sceneName;
        public Button buttonObj; // 해당 씬을 담당하는 버튼 오브젝트
        public string targetSpawnName;  //다음 씬에 갈때 찾을 스폰포인트 이름
    }

    private void Start()
    {
        // 현재 씬 이름을 확인하여 로비 모드인지 판단
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Main Menu" || currentScene == "Lobby")
        {
            isLobbyMode = true;
        }

        // 닫기 버튼 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMapPanel);
        }
    }

    // LobbyManager가 OpenDepthPanel로 이 패널을 켜면 자동으로 실행됨
    private void OnEnable()
    {
        InitializeMap();
    }

    private void Update()
    {
        // 실시간으로 핀이 화면 밖으로 나갔는지 체크
        UpdateIndicators();
    }

    // 초기화 로직
    private void InitializeMap()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        // 로비가 아닐 때만(인게임) 시간을 멈추고 커서 처리
        if (!isLobbyMode)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        UpdatePlayerIconPosition();
        CreatePinsAndIndicators();

        // 현재 씬에 맞는 버튼 찾아서 포커스 (없으면 첫 번째 버튼)
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

        if (startButton == null && mapEntries.Count > 0)
        {
            startButton = mapEntries[0].buttonObj;
        }

        if (startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    //핀과 말풍선 만들기
    void CreatePinsAndIndicators()
    {
        // 기존에 만든 게 있다면 삭제 (중복 생성 방지)
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

            // 핀 생성 (지도 이미지의 자식이어야 함 -> 버튼과 형제거나 자식)
            // 버튼의 부모(Map_Image)를 찾아서 거기에 생성해야 스크롤될 때 같이 움직임
            GameObject newPin = Instantiate(pinPrefab, entry.buttonObj.transform.parent);
            newPin.transform.position = entry.buttonObj.transform.position; // 위치 맞춤
            newPin.transform.localPosition += pinOffset; // 살짝 위로 올림

            // 핀 스크립트 초기화
            UI_MapPin pinScript = newPin.GetComponent<UI_MapPin>();
            if (pinScript) pinScript.Setup();

            // 버튼 스크립트(UI_MapBuilding)에 핀 연결
            UI_MapBuilding buildingScript = entry.buttonObj.GetComponent<UI_MapBuilding>();
            if (buildingScript) buildingScript.linkedPin = pinScript;

            // 말풍선 생성 (지도 패널의 자식이어야 함 -> ScrollView 바깥)
            GameObject newIndicator = Instantiate(indicatorPrefab, indicatorParent);
            newIndicator.SetActive(false); // 처음엔 숨김

            // 리스트에 등록
            MapTarget newTarget = new MapTarget();
            newTarget.buttonTrans = entry.buttonObj.transform;
            newTarget.pinObj = newPin;
            newTarget.indicatorObj = newIndicator;
            newTarget.indicatorRect = newIndicator.GetComponent<RectTransform>();

            managedTargets.Add(newTarget);
        }
    }

    // 화면 밖인지 계산하고 말풍선 띄우기
    void UpdateIndicators()
    {
        if (viewportRect == null) return;

        // Viewport의 4모서리 월드 좌표를 가져옴
        Vector3[] corners = new Vector3[4];
        viewportRect.GetWorldCorners(corners);
        // corners[0] = bottom-left, [2] = top-right
        Rect viewRect = new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);

        foreach (var target in managedTargets)
        {
            if (target.buttonTrans == null) continue;

            Vector3 btnPos = target.buttonTrans.position;

            // 버튼이 Viewport 사각형 안에 있는지 확인
            bool isVisible = viewRect.Contains(btnPos);

            if (isVisible)
            {
                // 화면에 보이면 -> 핀은 켜고, 말풍선은 끈다
                if (target.pinObj) target.pinObj.SetActive(true);
                if (target.indicatorObj) target.indicatorObj.SetActive(false);
            }
            else
            {
                if (target.indicatorObj)
                {
                    target.pinObj.SetActive(false);
                    target.indicatorObj.SetActive(true);

                    // 말풍선 위치를 Viewport 가장자리에 고정 (Clamping)
                    Vector3 indicatorPos = btnPos;

                    // X축 Clamp (좌우) - 패딩값 20 정도 줌
                    indicatorPos.x = Mathf.Clamp(indicatorPos.x, viewRect.x + 30, viewRect.xMax - 30);
                    // Y축 Clamp (상하)
                    indicatorPos.y = Mathf.Clamp(indicatorPos.y, viewRect.y + 30, viewRect.yMax - 30);

                    target.indicatorObj.transform.position = indicatorPos;

                    // (선택) 말풍선 화살표 회전 로직은 복잡해지니 생략하고, 
                    // 그냥 둥둥 떠있는 아이콘 형태로 표시
                }
            }
        }
    }

    public void UpdatePlayerIconPosition()
    {
        if (playerIcon == null) return;

        string currentScene = SceneManager.GetActiveScene().name;
        bool foundLocation = false;

        foreach (var entry in mapEntries)
        {
            // 현재 씬과 일치하는 버튼을 찾음
            if (entry.sceneName == currentScene)
            {
                if (entry.buttonObj != null)
                {
                    playerIcon.SetActive(true);

                    // 아이콘을 버튼 위치로 이동 (World Position 기준)
                    playerIcon.transform.position = entry.buttonObj.transform.position;

                    // 오프셋 적용 (버튼 정중앙보다는 살짝 위가 보기 좋음)
                    // transform.position은 월드 좌표라 픽셀 단위 오프셋은 localPosition으로 하는 게 안전하지만,
                    // 간단하게 월드 좌표에 더해줘도 UI Canvas가 Screen Space라면 괜찮습니다.
                    playerIcon.transform.localPosition += iconOffset;

                    // 3. 맨 위에 그리기 (다른 버튼에 가려지지 않게)
                    playerIcon.transform.SetAsLastSibling();

                    foundLocation = true;
                }
                break;
            }
        }

        // 만약 현재 씬에 해당하는 버튼이 없다면 아이콘 숨기기 (예: 맵에 없는 던전에 있을 때)
        if (!foundLocation)
        {
            playerIcon.SetActive(false);
        }
    }

    // (외부 호출용) 기존 코드 호환성을 위해 남겨둠
    public void OpenMapPanel()
    {
        gameObject.SetActive(true);
        InitializeMap();
    }

    public void OnClickMapButton(int index)
    {
        if (index < 0 || index >= mapEntries.Count) return;

        currentTargetEntry = mapEntries[index];
        lastSelectedMapButton = EventSystem.current.currentSelectedGameObject;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            if (confirmText != null)
            {
                string displayName = string.IsNullOrEmpty(currentTargetEntry.locationName) ? currentTargetEntry.sceneName : currentTargetEntry.locationName;
                confirmText.text = $"{displayName}으로 이동하시겠습니까?";
            }
            EventSystem.current.SetSelectedGameObject(confirmYesButton.gameObject);
        }
        else
        {
            // 확인 패널이 없으면 바로 이동
            OnConfirmYes();
        }
    }

    public void OnConfirmYes()
    {
        // 씬 이동 전 시간 복구
        Time.timeScale = 1f;

        // 스폰 포인트 설정 (SceneTransferManager가 있다면)
        // (주의: SceneTransferManager 스크립트가 static 변수를 가지고 있어야 함)
        // SceneTransferManager.TargetSpawnName = currentTargetEntry.targetSpawnName;

        SceneManager.LoadScene(currentTargetEntry.sceneName);
    }

    public void OnConfirmNo()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        if (lastSelectedMapButton != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedMapButton);
        }
    }

    public void CloseMapPanel()
    {
        Time.timeScale = 1f;
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        // 로비 모드라면 LobbyManager의 스택 시스템을 이용해서 뒤로가기
        if (isLobbyMode && LobbyManager.instance != null)
        {
            LobbyManager.instance.OnClickBack();
        }
        else
        {
            // 인게임이라면 그냥 패널 끄기
            gameObject.SetActive(false);

            // 인게임 커서 잠금 복구 (필요시 주석 해제)
            // if (!isLobbyMode) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        }
    }
}