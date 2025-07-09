using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuickSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public Text SlotNum;
    public Text amountText;
    public int slotIndex = 0;

    private InventoryItem assignedItem;
    public Sprite emptySlotSprite;

    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;


    void Start()
    {
        SlotNum.text = (slotIndex + 1).ToString();

        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UseItem();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (assignedItem == null) return;

        DragIconUI.Instance.Show(assignedItem.itemData.icon);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragIconUI.Instance.Hide();
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        //추가
        QuickSlotUI draggedQuickSlot = eventData.pointerDrag?.GetComponent<QuickSlotUI>();
        if (draggedQuickSlot != null)
        {
            var fromItem = draggedQuickSlot.GetAssignedItem();
            var toItem = this.assignedItem;

            // 교환
            draggedQuickSlot.SetAssignedItem(toItem);
            this.SetAssignedItem(fromItem);
        }

        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot != null)
        {
            InventoryItem droppedItem = draggedSlot.GetItem();
            QuickSlotManager.Instance.AssignItemToSlot(droppedItem, slotIndex);
        }
    }

    public InventoryItem GetAssignedItem()
    {
        return assignedItem;
    }

    public void SetAssignedItem(InventoryItem item)
    {
        assignedItem = item;
        UpdateSlotIcon();
    }

    public void ClearSlot()
    {
        assignedItem = null;
        if (emptySlotSprite != null)
        {
            icon.sprite = emptySlotSprite;
            icon.enabled = true;
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        amountText.text = "";
        amountText.gameObject.SetActive(false);
    }

    public void UseItem()
    {
        if (assignedItem == null) return;


        ItemData data = assignedItem.itemData;

        //예시: 회복 아이템
        if (data.itemType == ItemType.Potion)
        {
            PlayerControl player = FindObjectOfType<PlayerControl>();
            if (player != null)
            {
                if (data.itemID == 0)
                {
                    player.Heal(data.amount);
                    Debug.Log($"[{slotIndex + 1}] 포션 사용! HP +{data.amount}");
                }
                if (data.itemID == 1)
                {
                    player.RecoverMp(data.amount);
                    Debug.Log($"[{slotIndex + 1}] 포션 사용! MP +{data.amount}");
                }
            }
        }
        //예시: 무기
        else if (data.itemType == ItemType.Weapon)
        {
            Debug.Log($"[{slotIndex}] 무기 '{data.itemName}' 사용");
            // 무기 장착/사용 로직 여기에
        }

        //소모형 아이템이면 개수 감소
        if (data.isConsumable)
        {
            assignedItem.count--;
            if (assignedItem.count <= 0)
            {
                ClearSlot();
            }
            else
            {
                UpdateSlotIcon(); // 수량 갱신
            }
        }
    }

    public void UpdateSlotIcon()
    {
        if (assignedItem != null)
        {
            icon.sprite = assignedItem.itemData.icon;
            icon.enabled = true;

            if (assignedItem.count > 1)
            {
                amountText.text = assignedItem.count.ToString();
                amountText.gameObject.SetActive(true);
            }
            else
            {
                amountText.text = "";
                amountText.gameObject.SetActive(false);
            }

        }
        else
        {
            icon.sprite = emptySlotSprite;
            icon.enabled = true;
            amountText.text = "";
            amountText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition))
        {
            //Debug.Log($"[DEBUG] 마우스가 퀵슬롯 {slotIndex} 위에 있음");
        }
    }
}