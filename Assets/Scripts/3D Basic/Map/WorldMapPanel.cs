using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapPanel : MonoBehaviour
{
    public RectTransform mapImage; // 전체맵 이미지 UI
    public RectTransform playerIcon;
    public Transform player;
    public Transform target;

    // 맵 범위(직접 측정하거나 Gizmo로 확인)
    public float worldMinX;
    public float worldMaxX;
    public float worldMinZ;
    public float worldMaxZ;

    private void OnEnable() // 패널이 켜질 때마다 확인
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;
        Vector2 mapPos = WorldToMapPosition(player.position);

        playerIcon.anchoredPosition = mapPos;

        // 아이콘 방향도 맞추고 싶으면
        playerIcon.localEulerAngles = new Vector3(0, 0, -target.eulerAngles.y);
    }

    public Vector2 WorldToMapPosition(Vector3 worldPos)
    {
        float normalizedX = (worldPos.x - worldMinX) / (worldMaxX - worldMinX);
        float normalizedY = (worldPos.z - worldMinZ) / (worldMaxZ - worldMinZ);

        float mapX = normalizedX * mapImage.rect.width;
        float mapY = normalizedY * mapImage.rect.height;

        return new Vector2(mapX, mapY);
    }
}
