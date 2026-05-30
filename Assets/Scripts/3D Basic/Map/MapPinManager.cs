using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPinManager : MonoBehaviour
{
    public static MapPinManager instance;

    [Header("핀 생성 패널 (Create)")]
    public GameObject createPanel;
    public InputField descInput;
    public int currentSelectedIconIndex = 0; // UI 버튼들로 이 숫자를 0, 1, 2...로 바꿔주면 됨

    [Header("핀 테두리 UI 설정")]
    public Image[] pinButtonBorders;
    public Color normalBorderColor = Color.gray;  // 기본 테두리 색상
    public Color selectedBorderColor = Color.green;

    [Header("핀 상세 패널 (Details)")]
    public GameObject detailsPanel;
    public Text detailsDescText;
    public Image detailsIconPreview;

    [Header("상세 제어 UI")]
    public GameObject deleteButton;   // 삭제 버튼 오브젝트
    public Text trackButtonText;

    [Header("핀 프리팹 및 아이콘 데이터")]
    public GameObject pinPrefab; // Content 자식으로 생성될 UI 핀
    public Transform mapContent;
    public Sprite[] pinIcons; // 별, 몬스터, 광석 등 다양한 핀 아이콘 배열

    [Header("3D 추적용 더미 타겟")]
    [Tooltip("3D 씬에 만들어둔 빈 GameObject를 연결하세요.")]
    public Transform dummy3DWaypoint;

    [HideInInspector] public List<UICustomPin> spawnedPins = new List<UICustomPin>(); // 생성된 핀들을 기억할 명부
    public System.Action OnPinListChanged;

    private Vector2 pendingMapPos;
    private Vector2 currentTargetMapPos;
    private Sprite currentTargetSprite;

    private UICustomPin currentSelectedPin;

    private void Awake() { instance = this; }

    // 지도 빈 곳을 클릭했을 때 (MapClickDetector가 호출)
    public void OpenCreatePanel(Vector2 mapPos)
    {
        pendingMapPos = mapPos;
        createPanel.SetActive(true);
        detailsPanel.SetActive(false);
        if (descInput != null) descInput.text = "";

        SelectIcon(0);
    }

    // 아이콘 선택 버튼을 눌렀을 때 (UI 버튼 이벤트에 연결)
    public void SelectIcon(int index)
    {
        currentSelectedIconIndex = index;
        UpdateIconSelectionUI(); // 색상 변경 함수 호출
    }

    private void UpdateIconSelectionUI()
    {
        for (int i = 0; i < pinButtonBorders.Length; i++)
        {
            if (pinButtonBorders[i] != null)
            {
                pinButtonBorders[i].color = (i == currentSelectedIconIndex) ? selectedBorderColor : normalBorderColor;
            }
        }
    }

    // [확인] 버튼 클릭 시
    public void ConfirmCreate()
    {
        GameObject newPin = Instantiate(pinPrefab, mapContent);
        newPin.GetComponent<RectTransform>().anchoredPosition = pendingMapPos;

        UICustomPin pinScript = newPin.GetComponent<UICustomPin>();
        Sprite selectedSprite = pinIcons[currentSelectedIconIndex];
        pinScript.Setup(pendingMapPos, currentSelectedIconIndex, descInput.text, selectedSprite);

        spawnedPins.Add(pinScript);
        OnPinListChanged?.Invoke();

        createPanel.SetActive(false);
    }

    // [취소] 버튼 클릭 시
    public void CancelCreate() 
    { 
        createPanel.SetActive(false); 
    }

    public void OpenDetailsPanel(string desc, Sprite icon, Vector2 pos, UICustomPin customPin = null)
    {
        currentSelectedPin = customPin;
        currentTargetMapPos = pos;
        currentTargetSprite = icon;

        detailsDescText.text = desc;
        detailsIconPreview.sprite = icon;

        // 1. 내가 직접 생성한 핀일 때만 [삭제] 버튼을 활성화시킵니다.
        if (deleteButton != null)
        {
            deleteButton.SetActive(customPin != null);
        }

        // 2. 현재 이 핀이 추적 중인지 체크하여 버튼 글자 실시간 업데이트
        UpdateTrackButtonText();

        createPanel.SetActive(false);
        detailsPanel.SetActive(true);
    }

    public void CloseDetailsPanel() { detailsPanel.SetActive(false); }

    public void UpdateTrackButtonText()
    {
        if (trackButtonText == null) return;
        trackButtonText.text = IsCurrentPinTracked() ? "추적 취소" : "추적";
    }

    private bool IsCurrentPinTracked()
    {
        if (QuestMarkerUI.instance == null || QuestMarkerUI.instance.CurrentTarget != dummy3DWaypoint)
            return false;

        Vector3 worldPos = LocalMapController.instance.GetWorldPosition(currentTargetMapPos);

        float distXZ = Vector2.Distance(new Vector2(dummy3DWaypoint.position.x, dummy3DWaypoint.position.z), new Vector2(worldPos.x, worldPos.z));
        return distXZ < 0.2f;
    }

    public void TrackCurrentPin()
    {
        if (dummy3DWaypoint == null) return;

        if (IsCurrentPinTracked())
        {
            if (QuestMarkerUI.instance != null) QuestMarkerUI.instance.ClearTarget();
            UpdateTrackButtonText();
            return;
        }

        // 새로운 추적 개시
        Vector3 worldPos = LocalMapController.instance.GetWorldPosition(currentTargetMapPos);
        dummy3DWaypoint.position = new Vector3(worldPos.x, 2f, worldPos.z);

        if (QuestMarkerUI.instance != null)
        {
            QuestMarkerUI.instance.SetTarget(dummy3DWaypoint, currentTargetSprite);
        }

        detailsPanel.SetActive(false);
        LocalMapController.instance.CloseLocalMap();
    }

    public void DeleteCurrentPin()
    {
        if (currentSelectedPin == null) return;

        if (IsCurrentPinTracked() && QuestMarkerUI.instance != null)
        {
            QuestMarkerUI.instance.ClearTarget();
        }

        Destroy(currentSelectedPin.gameObject);
        currentSelectedPin = null;

        detailsPanel.SetActive(false);
    }

    // 미니맵 새로고침 이벤트를 안전하게 터트려주는 함수
    public void NotifyPinListChanged()
    {
        OnPinListChanged?.Invoke();
    }

    // 현재 추적 중이었던 위치의 핀을 찾아서 파괴하는 함수 (도착 트리거용)
    public void DestroyTrackedPinAtCurrentTarget()
    {
        // 명부에서 현재 추적 중인 2D 맵 좌표(currentTargetMapPos)와 일치하는 핀을 검색
        UICustomPin targetPin = spawnedPins.Find(p => Vector2.Distance(p.mapPos, currentTargetMapPos) < 0.5f);
        if (targetPin != null)
        {
            // 핀을 파괴합니다. 
            // 파괴되는 순간 1단계에서 만든 OnDestroy()가 실행되면서 리스트 제거와 미니맵 갱신까지 세트로 처리됩니다!
            Destroy(targetPin.gameObject);
        }
    }
}