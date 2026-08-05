using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image ItemIcon;
    public Text quantityText;

    [Header("Slot Info")]
    private ItemHolder assignedItemHolder;  //슬롯에 담긴 아이템 정보
    private SlotType slotType;
    private int originalInventoryIndex;

    private UI_Inventory uiInventory;

    private bool isDraggable = true;

    public bool isEmpty => assignedItemHolder == null;

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

        if (quantityText != null)
        {
            if (itemHolder.Quantity > 1)
            {
                quantityText.text = itemHolder.Quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
    }

    public void Clear()
    {
        assignedItemHolder = null;
        ItemIcon.sprite = null;
        ItemIcon.gameObject.SetActive(false);
        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || isEmpty) return;
        if (uiInventory != null)
        {
            uiInventory.CloseDetailPanel();
        }
        DragSlot.StartDrag(ItemIcon, assignedItemHolder, originalInventoryIndex, this.slotType);
        ItemIcon.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable || isEmpty) return;
        DragSlot.StartDrag(ItemIcon, this.slotType, this.originalInventoryIndex, this.slotType);
        DragSlot.dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (uiInventory.enableDrag != false)
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (uiInventory != null && uiInventory.showDetailOnHover && !isEmpty)
        {
            uiInventory.UpdateDetailView(assignedItemHolder);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiInventory != null && uiInventory.showDetailOnHover)
        {
            uiInventory.CloseDetailPanel();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isEmpty) return;

        if (uiInventory != null && !uiInventory.showDetailOnHover)
        {
            uiInventory.UpdateDetailView(assignedItemHolder);
        }
    }
}
