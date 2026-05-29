using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LocalMapController : MonoBehaviour
{
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

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerPositionTarget = playerObj.transform;

            // 2. 그 자식 중에 진짜 회전하는 녀석을 찾아서 회전 타겟으로 지정
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
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollWheel) > 0.01f)
        {
            float currentScale = mapContent.localScale.x;

            if (mapViewport != null && mapContent != null)
            {
                float minScaleX = mapViewport.rect.width / mapContent.rect.width;
                float minScaleY = mapViewport.rect.height / mapContent.rect.height;

                // 가로/세로 중 화면을 꽉 채울 수 있는 '더 큰 비율'을 진짜 최저 한계선으로 잡습니다.
                float calculatedMinZoom = Mathf.Max(minScaleX, minScaleY);

                // 인스펙터에 적어둔 minZoom이 너무 낮다면 계산된 안전한 배율로 덮어씌웁니다.
                if (minZoom < calculatedMinZoom)
                {
                    minZoom = calculatedMinZoom;
                }
            }

            // 안전장치가 적용된 minZoom과 maxZoom 사이로 스케일 제어
            float newScale = Mathf.Clamp(currentScale + (scrollWheel * zoomSpeed), minZoom, maxZoom);
            mapContent.localScale = new Vector3(newScale, newScale, 1f);
        }
    }

    private void UpdatePlayerIcon()
    {
        if (playerPositionTarget == null || playerIconRect == null) return;

        // 위치는 최상위 부모(Character)의 좌표를 사용
        Vector2 mapPos = GetMapPosition(playerPositionTarget.position);
        playerIconRect.anchoredPosition = mapPos;

        // 회전은 자식 모델(Player)의 각도를 사용
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

            if (playerPositionTarget != null)
            {
                Vector2 playerMapPos = GetMapPosition(playerPositionTarget.position);
                // 맵 도화지를 플레이어 위치의 반대 방향으로 밀어주어야 화면 중앙에 플레이어가 오게 됩니다.
                mapContent.anchoredPosition = -playerMapPos * initialZoom;
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

    private void TeleportPlayer(Transform targetTr)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            UnityEngine.AI.NavMeshAgent agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            player.transform.position = targetTr.position;
            player.transform.rotation = targetTr.rotation;

            Transform cameraArm = player.transform.Find("CameraArm");
            if (cameraArm == null)
            {
                var camScript = player.GetComponentInChildren<CameraArm>();
                if (camScript != null) cameraArm = camScript.transform;
            }
            if (cameraArm != null) cameraArm.rotation = targetTr.transform.rotation;

            if (agent != null) agent.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        CloseLocalMap();
    }

    public void CloseLocalMap()
    {
        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.CloseSpecificUI(this.gameObject);
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
}