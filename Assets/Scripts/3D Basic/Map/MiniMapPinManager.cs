using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapPinManager : MonoBehaviour
{
    [Header("미니맵 핀 프리팹")]
    [Tooltip("미니맵 Content 자식으로 생성될 커스텀 핀 프리팹")]
    public GameObject miniMapPinPrefab;

    [Header("미니맵 Content 연결")]
    public RectTransform miniMapContent;

    private List<GameObject> activeMiniMapPins = new List<GameObject>();

    private void Start()
    {
        EnsureMapPinManagerInstance();

        if (MapPinManager.instance != null)
        {
            MapPinManager.instance.OnPinListChanged += RefreshMiniMapPins;
        }

        RefreshMiniMapPins();
    }

    private void OnDestroy()
    {
        if (MapPinManager.instance != null)
        {
            MapPinManager.instance.OnPinListChanged -= RefreshMiniMapPins;
        }
    }

    private void OnEnable()
    {
        RefreshMiniMapPins();
    }

    private void EnsureMapPinManagerInstance()
    {
        if (MapPinManager.instance == null)
        {
            MapPinManager.instance = FindObjectOfType<MapPinManager>(true);
        }
    }

    public void RefreshMiniMapPins()
    {
        EnsureMapPinManagerInstance();

        // 기존에 미니맵 위에 존재하던 가짜 핀들을 청소
        foreach (GameObject pin in activeMiniMapPins)
        {
            if (pin != null) Destroy(pin);
        }
        activeMiniMapPins.Clear();

        if (MapPinManager.instance == null || miniMapContent == null || miniMapPinPrefab == null) return;

        // 전체 맵 명부(spawnedPins)에 적힌 핀들을 하나씩 미니맵용으로 복제
        foreach (UICustomPin mainPin in MapPinManager.instance.spawnedPins)
        {
            if (mainPin == null) continue;

            GameObject newMiniPin = Instantiate(miniMapPinPrefab, miniMapContent);
            activeMiniMapPins.Add(newMiniPin);

            Image img = newMiniPin.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = MapPinManager.instance.pinIcons[mainPin.iconIndex];
                img.raycastTarget = false; // 미니맵 핀은 마우스 클릭 감지 해제
            }

            RectTransform rect = newMiniPin.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;

                rect.anchoredPosition = mainPin.mapPos;
            }
        }
    }
}