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
    private bool isInputSlot;

    private int currentPlacedAmount;

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

    public ItemHolder GetAssignedItem()
    {
        return assignedItem;
    }

    public void Setup(ItemHolder item, int placedAmount = 0)
    {
        assignedItem = item;
        currentPlacedAmount = placedAmount; 

        if (item == null || item.ItemData == null)
        {
            Clear();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = item.ItemData.itemIcon;
            iconImage.gameObject.SetActive(true);

            if (!isInputSlot && placedAmount > 0)
            {
                iconImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            }
            else
            {
                iconImage.color = Color.white;
            }
        }

        if (quantityText != null)
        {
            if (item.ItemData.itemType == ITEMTYPE.Equipment)
            {
                quantityText.gameObject.SetActive(false);
            }
            else
            {
                quantityText.gameObject.SetActive(true);

                if (isInputSlot)
                {
                    quantityText.text = item.Quantity.ToString();
                }
                else
                {
                    if (placedAmount > 0)
                        quantityText.text = $"<color=yellow>{placedAmount}</color>/{item.Quantity}";
                    else
                        quantityText.text = item.Quantity.ToString();
                }
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
        currentPlacedAmount = 0;
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
            iconImage.color = Color.white;
        }
        if (quantityText != null) quantityText.gameObject.SetActive(false);
        if (removeButton != null) removeButton.gameObject.SetActive(false);
    }

    private void OnSlotClick()
    {
        if (isInputSlot)
        {
            if (UI_WeaponEnhancement.instance != null && UI_WeaponEnhancement.instance.enhancementSubPanel.activeSelf)
                UI_WeaponEnhancement.instance.OpenMyMaterialInventory();
        }
        else
        {
            if (assignedItem != null)
            {
                if (UI_WeaponEnhancement.instance != null && UI_WeaponEnhancement.instance.enhancementSubPanel.activeSelf)
                {
                    UI_WeaponEnhancement.instance.AddMaterialFromInventory(assignedItem);
                }
                else if (UI_WeaponRefinement.instance != null && !UI_WeaponEnhancement.instance.enhancementSubPanel.activeSelf)
                {
                    if (currentPlacedAmount > 0)
                    {
                        UI_WeaponRefinement.instance.RemoveMaterialFromInput(assignedItem);
                    }
                    else
                    {
                        UI_WeaponRefinement.instance.AddMaterialToSlot(assignedItem);
                    }
                }
            }
        }
    }

    private void OnRemoveClick()
    {
        if (isInputSlot && assignedItem != null)
        {
            if (UI_WeaponEnhancement.instance != null && UI_WeaponEnhancement.instance.enhancementSubPanel.activeSelf)
                UI_WeaponEnhancement.instance.RemoveMaterialFromInput(assignedItem);
            else if (UI_WeaponRefinement.instance != null && !UI_WeaponEnhancement.instance.enhancementSubPanel.activeSelf)
                UI_WeaponRefinement.instance.RemoveMaterialFromInput(assignedItem);
        }
    }
}