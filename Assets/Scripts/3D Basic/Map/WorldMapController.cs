using UnityEngine;
using UnityEngine.UI;

public class WorldMapIconUpdater : MonoBehaviour
{
    public RectTransform mapImage;      // 전체맵 이미지
    public RectTransform playerIcon;    // 플레이어 아이콘

    public Transform player;            // 실제 플레이어

    public Vector2 worldMin;            // 월드 최소(x,z)
    public Vector2 worldMax;            // 월드 최대(x,z)

    void Update()
    {
        UpdatePlayerIcon();
    }

    void UpdatePlayerIcon()
    {
        // 1) 월드좌표 → 0~1 비율
        Vector2 normalized = new Vector2(
            Mathf.InverseLerp(worldMin.x, worldMax.x, player.position.x),
            Mathf.InverseLerp(worldMin.y, worldMax.y, player.position.z)
        );

        // 2) 맵 이미지 크기
        Vector2 mapSize = mapImage.rect.size;

        // 3) 중앙을 (0,0)으로 맞춰 UI 좌표로 변환
        Vector2 uiPos = new Vector2(
            (normalized.x - 0.5f) * mapSize.x,
            (normalized.y - 0.5f) * mapSize.y
        );

        playerIcon.anchoredPosition = uiPos;
    }
}
