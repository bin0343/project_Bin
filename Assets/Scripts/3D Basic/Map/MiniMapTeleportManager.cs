using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapTeleportManager : MonoBehaviour
{
    public static MiniMapTeleportManager instance;

    [Header("미니맵 Content 연결")]
    public RectTransform miniMapContent;

    [Header("미니맵 워프 마커 프리팹")]
    public GameObject miniMapTeleportPrefab;

    [Header("상태별 시각 효과 (전체맵과 동일하게)")]
    public Color inactiveColor = Color.black;
    public Color activeColor = Color.red;

    private List<GameObject> activeMiniMarkers = new List<GameObject>();

    private void Awake() { instance = this; }

    private void Start()
    {
        RefreshMiniMapTeleports();
    }

    public void RefreshMiniMapTeleports()
    {
        if (LocalMapController.instance == null)
        {
            LocalMapController.instance = FindObjectOfType<LocalMapController>(true);
        }

        foreach (GameObject marker in activeMiniMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        activeMiniMarkers.Clear();

        if (LocalMapController.instance == null || miniMapContent == null || miniMapTeleportPrefab == null) return;

        TeleportPoint3D[] allTeleports = FindObjectsOfType<TeleportPoint3D>();

        foreach (TeleportPoint3D tp in allTeleports)
        {
            GameObject newMarker = Instantiate(miniMapTeleportPrefab, miniMapContent);
            activeMiniMarkers.Add(newMarker);

            RectTransform rect = newMarker.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one; // 크기 찌그러짐 방지

                rect.anchoredPosition = LocalMapController.instance.GetMapPosition(tp.transform.position);
            }

            Image markerImg = newMarker.GetComponent<Image>();
            if (markerImg != null)
            {
                markerImg.color = tp.isActivated ? activeColor : inactiveColor;
                markerImg.raycastTarget = false; // 미니맵 마커는 클릭 방해 금지
            }
        }
    }
}