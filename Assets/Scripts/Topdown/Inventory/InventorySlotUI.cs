using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public Text amountText;
    private InventoryItem currentItem;

    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private InventoryItem draggedItemCache;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (currentItem != null)
        {
            icon.sprite = currentItem.itemData.icon;
            icon.enabled = true;

            if (currentItem.count > 1)
            {
                amountText.text = $"X {currentItem.count}";
                amountText.enabled = true;
            }
            else if (currentItem.count == 1)
            {
                amountText.text = "";
                amountText.enabled = true;
            }
            else
            {
                ClearSlot();
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void SetItem(InventoryItem inventoryItem)
    {
        currentItem = inventoryItem;
        UpdateSlot();
    }

    public InventoryItem GetItem()
    {
        return currentItem;
    }

    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
        amountText.enabled = false;
        currentItem = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!icon.enabled) return;

        // 아이템 백업
        draggedItemCache = currentItem;

        originalParent = transform.parent;
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        icon.raycastTarget = false;

        eventData.pointerDrag = gameObject;

        //Debug.Log("BeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        //Debug.Log("OnDrag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
        canvasGroup.blocksRaycasts = true;
        icon.raycastTarget = true;

        if (draggedItemCache != null)
        {
            SetItem(draggedItemCache); // 아이템 되돌리기
            draggedItemCache = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot == null || draggedSlot == this) return;

        InventoryItem draggedItem = draggedSlot.GetItem();
        InventoryItem targetItem = this.GetItem();

        // 서로 교환
        draggedSlot.SetItem(targetItem);
        this.SetItem(draggedItem);

        draggedSlot.draggedItemCache = null;
    }
}