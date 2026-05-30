using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UICustomPin : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;

    [HideInInspector] public Vector2 mapPos;
    [HideInInspector] public int iconIndex;
    [HideInInspector] public string description;

    public void Setup(Vector2 pos, int index, string desc, Sprite iconSprite)
    {
        mapPos = pos;
        iconIndex = index;
        description = desc;
        if (iconImage != null) iconImage.sprite = iconSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.dragging && MapPinManager.instance != null)
        {
            // Manager에게 내 설명, 내 아이콘 이미지, 내 좌표를 넘기면서 열어달라고 요청!
            MapPinManager.instance.OpenDetailsPanel(description, iconImage.sprite, mapPos, this);
        }
    }

    private void OnDestroy()
    {
        if (MapPinManager.instance == null) return;

        // 1. 내가 전체 맵 명부(spawnedPins)에 기록된 '진짜 핀'인지 검사
        if (MapPinManager.instance.spawnedPins.Contains(this))
        {
            MapPinManager.instance.spawnedPins.Remove(this);

            MapPinManager.instance.NotifyPinListChanged();
        }
    }
}