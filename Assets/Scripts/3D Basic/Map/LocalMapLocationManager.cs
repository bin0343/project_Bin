using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalMapLocationManager : MonoBehaviour
{
    [Header("전체 지도 Content")]
    [SerializeField] private RectTransform mapContent;

    [Header("장소 마커 프리팹")]
    [SerializeField] private GameObject locationMarkerPrefab;

    private List<GameObject> activeMarkers = new List<GameObject>();


    private void OnEnable()
    {
        RefreshLocationMarkers();
    }


    public void RefreshLocationMarkers()
    {
        // 기존에 생성했던 지도 마커 제거
        foreach (GameObject marker in activeMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }

        activeMarkers.Clear();


        // 필수 참조 확인
        if (LocalMapController.instance == null || mapContent == null || locationMarkerPrefab == null)
        {
            return;
        }


        // 씬에 존재하는 MapLocationMarker 전부 검색
        MapLocationMarker[] allLocations = FindObjectsOfType<MapLocationMarker>(true);


        foreach (MapLocationMarker location in allLocations)
        {
            if (location == null) continue;

            if (!location.showOnLocalMap) continue;

            GameObject newMarker = Instantiate(locationMarkerPrefab, mapContent);

            activeMarkers.Add(newMarker);

            Image markerImage = newMarker.GetComponent<Image>();

            if (markerImage != null)
            {
                markerImage.sprite = location.mapIcon;
                markerImage.raycastTarget = true;
            }

            RectTransform markerRect = newMarker.GetComponent<RectTransform>();

            if (markerRect == null) continue;

            markerRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.localScale = Vector3.one;

            Vector2 mapPos = LocalMapController.instance.GetMapPosition(location.transform.position);

            markerRect.anchoredPosition = mapPos;

            UIMapInteractiveIcon interactScript = newMarker.GetComponent<UIMapInteractiveIcon>();

            if (interactScript == null)
            {
                interactScript = newMarker.AddComponent<UIMapInteractiveIcon>();
            }


            MapPinManager.IconGroupType groupType = location.canTeleport
                    ? MapPinManager.IconGroupType.Teleport
                    : MapPinManager.IconGroupType.General;


            string description = $"[{location.locationName}]\n{location.description}";


            interactScript.Setup(description, mapPos, groupType);


            if (location.canTeleport)
            {
                Transform teleportTarget = location.GetTeleportTarget();

                if (teleportTarget != null)
                {
                    interactScript.warpTargetName = teleportTarget.gameObject.name;
                }
            }
        }
    }
}