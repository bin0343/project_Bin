using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class QuestMarkerUI : MonoBehaviour
{
    public static QuestMarkerUI instance;

    public Transform CurrentTarget => currentTarget;

    [Header("UI 컴포넌트 연결")]
    public RectTransform markerRect;
    public Image markerIcon;
    public Text distanceText;

    [Header("추적 대상 설정")]
    public float heightOffset = 2.0f;
    public float screenPadding = 80f; // 화면 테두리 여백 (테두리에서 얼마나 떨어질지)

    [Header("원근감(3D 일체감) 설정")]
    public float maxScaleDistance = 5f;
    public float minScaleDistance = 50f;
    public float maxScale = 1.0f;
    public float minScale = 0.5f;

    [Header("추적 종료 설정")]
    public float arriveDistance = 1.0f; // 도착으로 인정할 거리 (1m)
    private Transform playerTransform;

    private Transform currentTarget;
    private Camera mainCam;
    private Sprite defaultIcon;

    private void Awake()
    {
        if (instance == null) instance = this;
        mainCam = Camera.main;
        if (markerIcon != null) defaultIcon = markerIcon.sprite;
        HideMarker();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        if (QuestManager.instance != null && !string.IsNullOrEmpty(QuestManager.instance.currentTrackedQuestID))
        {
            QuestManager.instance.RefreshTrackedQuestTarget();
        }
    }

    private void Update()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            if (markerRect.gameObject.activeSelf) HideMarker();
            return;
        }

        Transform currentPlayer = GetCurrentPlayerTransform();

        float distance = 0f;

        if (currentPlayer != null)
        {
            Vector3 playerPos = currentPlayer.position;
            Vector3 targetPos = currentTarget.position;

            playerPos.y = 0f;
            targetPos.y = 0f;

            distance = Vector3.Distance(playerPos, targetPos);
        }

        Vector3 worldPos = currentTarget.position + (Vector3.up * heightOffset);

        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        bool isOffScreen = screenPos.z < 0 ||
                           screenPos.x < 0 || screenPos.x > Screen.width ||
                           screenPos.y < 0 || screenPos.y > Screen.height;

        if (!markerRect.gameObject.activeSelf) ShowMarker();

        float distanceToCamera = Vector3.Distance(mainCam.transform.position, worldPos);
        float currentScale = maxScale;

        if (isOffScreen)
        {
            if (screenPos.z < 0)
            {
                screenPos *= -1;
            }

            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Vector3 dir = screenPos - screenCenter;
            dir.z = 0; // Z축 무시

            if (dir == Vector3.zero) dir = Vector3.up;

            float slope = dir.y / dir.x;

            float minX = screenPadding;
            float maxX = Screen.width - screenPadding;
            float minY = screenPadding;
            float maxY = Screen.height - screenPadding;

            Vector3 clampedPos = screenCenter;

            if (dir.x > 0)
            {
                clampedPos.x = maxX;
                clampedPos.y = screenCenter.y + (maxX - screenCenter.x) * slope;
            }
            else
            {
                clampedPos.x = minX;
                clampedPos.y = screenCenter.y + (minX - screenCenter.x) * slope;
            }

            if (clampedPos.y > maxY)
            {
                clampedPos.y = maxY;
                clampedPos.x = screenCenter.x + (maxY - screenCenter.y) / slope;
            }
            else if (clampedPos.y < minY)
            {
                clampedPos.y = minY;
                clampedPos.x = screenCenter.x + (minY - screenCenter.y) / slope;
            }

            screenPos = clampedPos;

            currentScale = minScale;
        }
        else
        {
            float scaleRatio = Mathf.InverseLerp(minScaleDistance, maxScaleDistance, distanceToCamera);
            currentScale = Mathf.Lerp(minScale, maxScale, scaleRatio);
        }

        markerRect.position = screenPos;
        markerRect.localScale = new Vector3(currentScale, currentScale, 1f);

        if (distanceText != null && currentPlayer != null)
        {
            distanceText.text = $"{Mathf.FloorToInt(distance)}m";
        }
    }

    private Transform GetCurrentPlayerTransform()
    {
        if (BattleManager.instance != null)
        {
            GameObject activePlayer = BattleManager.instance.GetActiveCharacter();

            if (activePlayer != null) return activePlayer.transform;
        }

        return playerTransform;
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
        ShowMarker();
    }

    public void ClearTarget()
    {
        currentTarget = null;
        HideMarker();
    }

    private void ShowMarker()
    {
        markerRect.gameObject.SetActive(true);
        markerRect.localScale = Vector3.zero;
        markerRect.DOScale(maxScale, 0.4f).SetEase(Ease.OutBack);
    }

    private void HideMarker()
    {
        markerRect.DOKill();
        markerRect.gameObject.SetActive(false);
    }

    public void SetTarget(Transform newTarget, Sprite overrideIcon = null)
    {
        currentTarget = newTarget;

        if (markerIcon != null)
        {
            markerIcon.sprite = (overrideIcon != null) ? overrideIcon : defaultIcon;
        }

        ShowMarker();
    }
}