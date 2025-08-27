using UnityEngine;
using UnityEngine.UI;

public static class DragSlot
{
    public static Image dragIcon;    // 마우스를 따라다닐 '고스트 아이콘' 이미지
    public static object draggedItem; // 드래그 중인 아이템/스킬 홀더 데이터
    public static int originalIndex; // 드래그를 시작한 슬롯의 인덱스
    public static SlotType originalSlotType;

    // 드래그 시작 시 호출
    public static void StartDrag(Image icon, object itemHolder, int index, SlotType type)
    {
        draggedItem = itemHolder;
        originalIndex = index;
        originalSlotType = type;

        dragIcon.sprite = icon.sprite;
        dragIcon.gameObject.SetActive(true);
    }

    // 드래그 종료 시 호출
    public static void EndDrag()
    {
        draggedItem = null;
        originalIndex = -1;

        dragIcon.gameObject.SetActive(false);
    }
}
