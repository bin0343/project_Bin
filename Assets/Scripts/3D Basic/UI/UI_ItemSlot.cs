using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Components")]
    public Image ItemIcon;
    public Text QuantityText;

    [Header("Slot Info")]
    private ItemHolder assignedItemHolder;
    private SlotType slotType;
    private int slotIndex;

    public bool IsEmpty => assignedItemHolder == null;

    public void Initialize(SlotType type, int index)
    {
        this.slotType = type;
        this.slotIndex = index;
    }

    public void Setup(ItemHolder itemHolder)
    {
        if (itemHolder == null || itemHolder.ItemData == null)
        {
            Clear();
            return;
        }

        assignedItemHolder = itemHolder;
        ItemIcon.sprite = itemHolder.ItemData.itemIcon;
        ItemIcon.gameObject.SetActive(true);

        if (itemHolder.Quantity > 1)
        {
            QuantityText.text = itemHolder.Quantity.ToString();
            QuantityText.gameObject.SetActive(true);
        }
        else
        {
            QuantityText.gameObject.SetActive(false);
        }
    }

    public void Clear()
    {
        assignedItemHolder = null;
        ItemIcon.gameObject.SetActive(false);
        QuantityText.gameObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;
        DragSlot.StartDrag(ItemIcon, assignedItemHolder, slotIndex, this.slotType);
        // 드래그 시작 시 자신의 슬롯 타입을 DragSlot에 저장 (중요!)
        // DragSlot 클래스에 public static SlotType originalSlotType; 추가 필요
        ItemIcon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;
        // 마우스 위치로 고스트 아이콘 이동
        DragSlot.dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝났음을 알리고 원래 아이콘을 다시 보이게 함
        DragSlot.EndDrag();
        if (!IsEmpty) ItemIcon.color = new Color(1, 1, 1, 1);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Player_Inventory.Instance.SwapSlots(
            DragSlot.originalSlotType, // 드래그 시작 슬롯의 타입
            DragSlot.originalIndex,    // 드래그 시작 슬롯의 인덱스
            this.slotType,             // 드롭된 위치(현재 슬롯)의 타입
            this.slotIndex             // 드롭된 위치(현재 슬롯)의 인덱스
        );
    }
}
