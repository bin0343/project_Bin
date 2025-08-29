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

    private UI_Inventory uiInventory;

    private bool isDraggable = true;

    public bool IsEmpty => assignedItemHolder == null;

    private void Awake()
    {
        uiInventory = GetComponentInParent<UI_Inventory>();
    }

    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;
    }

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
        if (!isDraggable || IsEmpty) return;
        DragSlot.StartDrag(ItemIcon, assignedItemHolder, originalInventoryIndex, this.slotType);
        ItemIcon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable || IsEmpty) return;
        DragSlot.StartDrag(ItemIcon, this.slotType, this.originalInventoryIndex, this.slotType);
        DragSlot.dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragSlot.EndDrag();
        ItemIcon.color = new Color(1, 1, 1, 1);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!isDraggable) return;

        if (DragSlot.originalSlotType == this.slotType && DragSlot.originalIndex == this.originalInventoryIndex)
        {
            return;
        }
        if (uiInventory != null)
        {
            Player_Inventory.Instance.HandleSlotDrop(
                DragSlot.originalSlotType,
                DragSlot.originalIndex,
                this.slotType,
                this.originalInventoryIndex,
                uiInventory.CurrentTab
            );
        }
        else
        {
            Player_Inventory.Instance.HandleSlotDrop(
                DragSlot.originalSlotType,
                DragSlot.originalIndex,
                this.slotType,
                this.originalInventoryIndex,
                UI_Inventory.InventoryTabType.ALL
            );
        }
    }
}
