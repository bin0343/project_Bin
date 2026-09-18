using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapLocationManager : MonoBehaviour
{
    [Header("¹Ì´Ï¸Ê Content")]
    [SerializeField] private RectTransform miniMapContent;

    [Header("Àå¼Ò ¸¶Ä¿ ÇÁ¸®ÆÕ")]
    [SerializeField] private GameObject miniMapLocationPrefab;

    private List<GameObject> activeMarkers = new List<GameObject>();


    private void Start()
    {
        RefreshLocationMarkers();
    }


    private void OnEnable()
    {
        RefreshLocationMarkers();
    }


    public void RefreshLocationMarkers()
    {
        foreach (GameObject marker in activeMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }

        activeMarkers.Clear();

        if (LocalMapController.instance == null)
        {
            LocalMapController.instance = FindObjectOfType<LocalMapController>(true);
        }


        if (LocalMapController.instance == null || miniMapContent == null || miniMapLocationPrefab == null)
        {
            return;
        }

        MapLocationMarker[] allLocations = FindObjectsOfType<MapLocationMarker>(true);


        foreach (MapLocationMarker location in allLocations)
        {
            if (location == null) continue;

            if (!location.isRevealed) continue;

            if (!location.showOnMiniMap) continue;

            GameObject newMarker = Instantiate(miniMapLocationPrefab, miniMapContent);

            activeMarkers.Add(newMarker);

            Image markerImage = newMarker.GetComponent<Image>();

            if (markerImage != null)
            {
                markerImage.sprite = location.mapIcon;

                markerImage.raycastTarget = false;
            }

            RectTransform markerRect = newMarker.GetComponent<RectTransform>();

            if (markerRect == null) continue;


            markerRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.localScale = Vector3.one;

            Vector2 mapPos = LocalMapController.instance.GetMapPosition(location.transform.position);

            markerRect.anchoredPosition = mapPos;
        }
    }
}