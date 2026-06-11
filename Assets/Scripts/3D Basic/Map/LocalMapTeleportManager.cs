using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalMapTeleportManager : MonoBehaviour
{
    public static LocalMapTeleportManager instance;

    [Header("UI 연결 설정")]
    public RectTransform mapContent; // 거대한 지도 도화지
    public GameObject teleportMarkerPrefab; // 워프 포인트 UI 프리팹

    [Header("상태별 시각 효과")]
    public Color inactiveColor = Color.black; // 미활성화 시 색상 (까맣게)
    public Color activeColor = Color.red;     // 활성화 시 색상 (빨갛게)

    private List<GameObject> activeMarkers = new List<GameObject>();

    private void Awake() { instance = this; }

    private void Start()
    {
        // 맵이 켜질 때 마커들을 그립니다.
        RefreshTeleportMarkers();
    }

    public void RefreshTeleportMarkers()
    {
        // 1. 기존에 그려둔 워프 마커들 싹 청소하기
        foreach (GameObject marker in activeMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        activeMarkers.Clear();

        if (LocalMapController.instance == null || mapContent == null || teleportMarkerPrefab == null) return;

        // 2. 3D 씬에 존재하는 모든 워프 포인트(전화박스)들을 싹 다 찾기
        TeleportPoint3D[] allTeleports = FindObjectsOfType<TeleportPoint3D>();

        // 3. 찾은 워프 포인트들의 좌표를 계산해서 지도에 하나씩 찍어주기
        foreach (TeleportPoint3D tp in allTeleports)
        {
            GameObject newMarker = Instantiate(teleportMarkerPrefab, mapContent);
            activeMarkers.Add(newMarker);

            // 위치 동기화 (3D 좌표 -> 2D 맵 좌표)
            RectTransform rect = newMarker.GetComponent<RectTransform>();
            rect.anchoredPosition = LocalMapController.instance.GetMapPosition(tp.transform.position);

            // 상태에 따른 색상 변경 (흑백 vs 컬러)
            Image markerImg = newMarker.GetComponent<Image>();
            if (markerImg != null)
            {
                markerImg.color = tp.isActivated ? activeColor : inactiveColor;
            }

            // 상호작용 스크립트 붙이기 (클릭 시 상세 패널 열기 위함)
            UIMapInteractiveIcon interactScript = newMarker.GetComponent<UIMapInteractiveIcon>();
            if (interactScript == null) interactScript = newMarker.AddComponent<UIMapInteractiveIcon>();

            var groupType = tp.isActivated ? MapPinManager.IconGroupType.Teleport : MapPinManager.IconGroupType.General;

            string statusText = tp.isActivated ? "\n<color=green>[활성화 됨]</color>" : "\n<color=red>[미활성화 - 다가가서 상호작용 필요]</color>";
            string finalDesc = tp.teleportDesc + "\n" + statusText;

            interactScript.Setup(finalDesc, rect.anchoredPosition, groupType);
            interactScript.warpTargetName = tp.gameObject.name;
        }
    }
}