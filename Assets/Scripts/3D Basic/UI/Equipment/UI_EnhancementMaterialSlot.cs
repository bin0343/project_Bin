using UnityEngine;
using UnityEngine.UI;

public class UI_EnhancementMaterialSlot : MonoBehaviour
{
    public Image iconImage;
    public Text quantityText;
    public Button clickButton;

    public Button removeButton;

    private ItemHolder assignedItem;
    private int slotIndex;
    private bool isInputSlot; // 투입구 슬롯인지, 가방 슬롯인지 분기점

    private void Awake()
    {
        if (clickButton == null) clickButton = GetComponent<Button>();
        clickButton.onClick.AddListener(OnSlotClick);

        if (removeButton != null)
        {
            removeButton.onClick.AddListener(OnRemoveClick);
            removeButton.gameObject.SetActive(false);
        }
    }

    public void Initialize(int index, bool isInput)
    {
        this.slotIndex = index;
        this.isInputSlot = isInput;
    }

    public void Setup(ItemHolder item)
    {
        assignedItem = item;
        if (item == null || item.ItemData == null)
        {
            Clear();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = item.ItemData.itemIcon;
            iconImage.gameObject.SetActive(true);
        }

        if (quantityText != null)
        {
            if (item.Quantity > 1)
            {
                quantityText.text = $"x{item.Quantity}";
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }

        if (isInputSlot && removeButton != null)
        {
            removeButton.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        assignedItem = null;
        if (iconImage != null) iconImage.gameObject.SetActive(false);
        if (quantityText != null) quantityText.gameObject.SetActive(false);

        if (removeButton != null) removeButton.gameObject.SetActive(false);
    }

    private void OnSlotClick()
    {
        if (isInputSlot)
        {
            if (UI_WeaponEnhancement.instance != null)
            {
                UI_WeaponEnhancement.instance.OpenMyMaterialInventory();
            }
        }
        else
        {
            if (assignedItem != null && UI_WeaponEnhancement.instance != null)
            {
                UI_WeaponEnhancement.instance.AddMaterialFromInventory(assignedItem);
            }
        }
    }

    private void OnRemoveClick()
    {
        if (isInputSlot && assignedItem != null && UI_WeaponEnhancement.instance != null)
        {
            UI_WeaponEnhancement.instance.RemoveMaterialFromInput(assignedItem);
        }
    }
}