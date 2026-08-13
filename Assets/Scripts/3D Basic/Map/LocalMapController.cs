using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LocalMapController : MonoBehaviour
{
    public static LocalMapController instance;

    [System.Serializable]
    public struct LocalLocationEntry
    {
        public string locationName;
        public Button buttonObj;
        public string warpPointName;
    }

    [Header("장소 목록 설정")]
    [SerializeField] private List<LocalLocationEntry> locations;

    [Header("UI 연결")]
    [SerializeField] private Text titleText;

    [Header("확인 패널")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Text confirmText;
    [SerializeField] private Button confirmYesButton;

    [Header("맵 뷰어 설정")]
    [SerializeField] private RectTransform mapContent; // 실제 맵 이미지가 들어있는 Content
    [SerializeField] private RectTransform mapViewport;
    [SerializeField] private float zoomSpeed = 0.5f;   // 휠 속도
    [SerializeField] private float initialZoom = 1.0f;
    [SerializeField] private float minZoom = 0.5f;     // 최소 축소 비율
    [SerializeField] private float maxZoom = 2.0f;     // 최대 확대 비율

    [Header("줌 슬라이더 UI")]
    [SerializeField] private Slider zoomSlider;

    [Header("3D 월드 공간 매핑 설정")]
    [Tooltip("3D 월드 맵의 중심점 좌표 (보통 0, 0, 0)")]
    public Vector3 worldCenter = Vector3.zero;
    [Tooltip("3D 월드의 실제 총 가로 크기(X축)와 세로 크기(Z축)")]
    public Vector2 worldSize = new Vector2(500f, 500f);

    [Header("플레이어 표시 설정")]
    [SerializeField] private RectTransform playerIconRect; // 맵 위에 띄울 플레이어 화살표 UI
    private Transform playerPositionTarget; // 위치 추적용 (최상위 부모 Character)
    private Transform playerRotationTarget; // 회전 추적용 (자식 모델링 Player)

    private int pendingTargetIndex = -1;
    private GameObject lastSelectedMapButton;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerPositionTarget = playerObj.transform;

            Transform realModel = playerObj.transform.Find("Player"); // 자식 모델의 실제 이름 입력
            if (realModel != null)
            {
                playerRotationTarget = realModel;
            }
            else
            {
                playerRotationTarget = playerObj.transform;
            }
        }

        if (zoomSlider != null)
        {
            zoomSlider.onValueChanged.AddListener(OnSliderZoomChanged);
        }
    }

    private void Update()
    {
        // 맵이 열려있을 때만 마우스 휠 줌(Zoom) 작동
        if (mapContent != null && gameObject.activeInHierarchy)
        {
            HandleMapZoom();
            UpdatePlayerIcon();
        }
    }

    private void HandleMapZoom()
    {
        if (mapViewport != null && mapContent != null)
        {
            float minScaleX = mapViewport.rect.width / mapContent.rect.width;
            float minScaleY = mapViewport.rect.height / mapContent.rect.height;
            float calculatedMinZoom = Mathf.Max(minScaleX, minScaleY);
            if (minZoom < calculatedMinZoom) minZoom = calculatedMinZoom;
        }

        if (zoomSlider != null)
        {
            zoomSlider.minValue = minZoom;
            zoomSlider.maxValue = maxZoom;
        }

        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollWheel) > 0.01f)
        {
            float currentScale = mapContent.localScale.x;
            float newScale = Mathf.Clamp(currentScale + (scrollWheel * zoomSpeed), minZoom, maxZoom);
            mapContent.localScale = new Vector3(newScale, newScale, 1f);

            if (zoomSlider != null)
            {
                zoomSlider.SetValueWithoutNotify(newScale);
            }
        }
    }

    private void OnSliderZoomChanged(float value)
    {
        if (mapContent == null) return;
        float newScale = Mathf.Clamp(value, minZoom, maxZoom);
        mapContent.localScale = new Vector3(newScale, newScale, 1f);
    }

    private void UpdatePlayerIcon()
    {
        if (BattleManager.Instance == null || playerIconRect == null) return;

        GameObject activePlayer = BattleManager.Instance.GetActiveCharacter();
        if (activePlayer == null) return;

        playerPositionTarget = activePlayer.transform;
        Transform realModel = activePlayer.transform.Find("Player"); 
        playerRotationTarget = (realModel != null) ? realModel : activePlayer.transform;

        Vector2 mapPos = GetMapPosition(playerPositionTarget.position);
        playerIconRect.anchoredPosition = mapPos;

        if (playerRotationTarget != null)
        {
            float playerRotationY = playerRotationTarget.eulerAngles.y;
            playerIconRect.localEulerAngles = new Vector3(0f, 0f, -playerRotationY);
        }
    }

    public Vector2 GetMapPosition(Vector3 worldPos)
    {
        if (mapContent == null) return Vector2.zero;

        // 3D 월드 중심점 기준으로 상대 좌표 계산
        float relativeX = worldPos.x - worldCenter.x;
        float relativeZ = worldPos.z - worldCenter.z;

        // 월드 크기 대비 비율 계산 (-0.5 ~ +0.5 범위를 0.0 ~ 1.0 비율로 정규화)
        float pctX = (relativeX / worldSize.x) + 0.5f;
        float pctZ = (relativeZ / worldSize.y) + 0.5f;

        // 0~1 사이의 비율을 2D UI 이미지(Content)의 실제 크기에 대입
        // UI의 중심점(Pivot)이 (0.5, 0.5)인 사각형이므로 크기 범위는 -Half ~ +Half 가 됩니다.
        float mapX = (pctX - 0.5f) * mapContent.rect.width;
        float mapY = (pctZ - 0.5f) * mapContent.rect.height;

        return new Vector2(mapX, mapY);
    }

    public void OpenLocalMap()
    {
        gameObject.SetActive(true);

        if (mapContent != null)
        {
            mapContent.localScale = new Vector3(initialZoom, initialZoom, 1f);

            if (zoomSlider != null)
            {
                zoomSlider.minValue = minZoom;
                zoomSlider.maxValue = maxZoom;
                zoomSlider.SetValueWithoutNotify(initialZoom);
            }

            if (BattleManager.Instance != null)
            {
                GameObject activePlayer = BattleManager.Instance.GetActiveCharacter();
                if (activePlayer != null)
                {
                    Vector2 playerMapPos = GetMapPosition(activePlayer.transform.position);
                    mapContent.anchoredPosition = -playerMapPos * initialZoom;
                }
            }
        }

        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        if (locations.Count > 0 && locations[0].buttonObj != null)
        {
            EventSystem.current.SetSelectedGameObject(locations[0].buttonObj.gameObject);
        }
    }

    public void OnClickTeleport(int index)
    {
        if (index < 0 || index >= locations.Count) return;

        pendingTargetIndex = index;
        lastSelectedMapButton = EventSystem.current.currentSelectedGameObject;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            if (confirmText != null) confirmText.text = $"{locations[index].locationName}(으)로\n이동하시겠습니까?";
            if (confirmYesButton != null) EventSystem.current.SetSelectedGameObject(confirmYesButton.gameObject);
        }
        else
        {
            ExecuteTeleport(index);
        }
    }

    public void OnConfirmYes()
    {
        if (pendingTargetIndex != -1) ExecuteTeleport(pendingTargetIndex);
    }

    public void OnConfirmNo()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (lastSelectedMapButton != null) EventSystem.current.SetSelectedGameObject(lastSelectedMapButton);
    }

    private void ExecuteTeleport(int index)
    {
        string targetName = locations[index].warpPointName;
        GameObject targetObj = GameObject.Find(targetName);

        if (targetObj != null)
        {
            TeleportPlayer(targetObj.transform);
        }
        else
        {
            Debug.LogError("이동할 목표 지점(Transform)이 연결되지 않았습니다!");
            if (confirmationPanel != null) confirmationPanel.SetActive(false);
        }
    }

    public void TeleportPlayer(Transform targetTr)
    {
        if (BattleManager.Instance == null) return;
        GameObject player = BattleManager.Instance.GetActiveCharacter();
        if (player == null) return;

        Vector3 finalPos = targetTr.position;
        Quaternion finalRot = targetTr.rotation;

        if (targetTr.GetComponent<TeleportPoint3D>() != null)
        {
            finalPos = targetTr.position - (targetTr.forward * 2.0f);
            finalRot = Quaternion.LookRotation(-targetTr.forward);
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero; // 1. 이전 위치에서 남아있던 가속도를 완전히 죽입니다.
            rb.position = finalPos;     // 2. 물리적 좌표 강제 순간이동
            rb.rotation = finalRot;     // 3. 물리적 회전 강제 순간이동
        }
        else
        {
            player.transform.position = finalPos;
            player.transform.rotation = finalRot;
        }
        
        Transform realModel = player.transform.Find("Player");
        if (realModel != null)
        {
            realModel.localRotation = Quaternion.identity;
        }

        if (BattleManager.Instance.mainFreeLookCamera != null)
        {
            BattleManager.Instance.mainFreeLookCamera.m_XAxis.Value = finalRot.eulerAngles.y;
            BattleManager.Instance.mainFreeLookCamera.m_YAxis.Value = 0.5f; // 중간 높이(정면) 응시

            BattleManager.Instance.mainFreeLookCamera.PreviousStateIsValid = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CloseLocalMap();
    }

    public void CloseLocalMap()
    {
        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.CloseSpecificUI(this.gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateDescription(int index)
    {
        if (titleText != null && index >= 0 && index < locations.Count)
        {
            titleText.text = locations[index].locationName;
        }
    }

    //맵UI에서 실제 맵으로 역산
    public Vector3 GetWorldPosition(Vector2 mapPos)
    {
        if (mapContent == null) return Vector3.zero;

        float pctX = (mapPos.x / mapContent.rect.width) + 0.5f;
        float pctZ = (mapPos.y / mapContent.rect.height) + 0.5f;

        float relativeX = (pctX - 0.5f) * worldSize.x;
        float relativeZ = (pctZ - 0.5f) * worldSize.y;

        return new Vector3(worldCenter.x + relativeX, 0f, worldCenter.z + relativeZ);
    }
}