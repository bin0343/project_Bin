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
    private int originalInventoryIndex;

    public bool IsEmpty => assignedItemHolder == null;

    public void Initialize(SlotType type, int index)
    {
        this.slotType = type;
        this.originalInventoryIndex = index;
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
        ItemIcon.sprite = null;
        ItemIcon.gameObject.SetActive(false);
        QuantityText.gameObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;
        DragSlot.StartDrag(ItemIcon, assignedItemHolder, originalInventoryIndex, this.slotType);
        // 드래그 시작 시 자신의 슬롯 타입을 DragSlot에 저장 (중요!)
        // DragSlot 클래스에 public static SlotType originalSlotType; 추가 필요
        ItemIcon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;
        DragSlot.StartDrag(ItemIcon, this.slotType, this.originalInventoryIndex, this.slotType);
        DragSlot.dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝났음을 알리고 원래 아이콘을 다시 보이게 함
        DragSlot.EndDrag();
        ItemIcon.color = new Color(1, 1, 1, 1);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (DragSlot.originalSlotType == this.slotType && DragSlot.originalIndex == this.originalInventoryIndex)
        {
            return; // 불필요한 로직 실행 방지
        }

        Player_Inventory.Instance.HandleSlotDrop(
            DragSlot.originalSlotType, // 드래그 시작 슬롯의 타입
            DragSlot.originalIndex,    // 드래그 시작 슬롯의 인덱스
            this.slotType,             // 드롭된 위치(현재 슬롯)의 타입
            this.originalInventoryIndex             // 드롭된 위치(현재 슬롯)의 인덱스
        );
    }
}
