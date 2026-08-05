using UnityEngine;
using UnityEngine.UI;

public class UI_ExpMaterialSlot : MonoBehaviour
{
    public Image iconImage;
    public Text countText;
    public Button clickButton;
    public Button minusButton;

    private ItemHolder assignedItem;
    public int usedCount {  get; private set; }

    private void Awake()
    {
        if (clickButton != null) clickButton.onClick.AddListener(OnClickAdd);
        if (minusButton != null) minusButton.onClick.AddListener(OnClickMinus);
    }

    public void Setup(ItemHolder item, int currentUsed = 0)
    {
        assignedItem = item;
        usedCount = currentUsed;

        if (item == null || item.ItemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (iconImage != null) iconImage.sprite = item.ItemData.itemIcon;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (usedCount > 0)
        {
            countText.text = $"<color=yellow>{usedCount}</color> / {assignedItem.Quantity}";
            if (minusButton != null) minusButton.gameObject.SetActive(true);
        }
        else
        {
            countText.text = assignedItem.Quantity.ToString();
            if (minusButton != null) minusButton.gameObject.SetActive(false);
        }
    }

    private void OnClickAdd()
    {
        if (assignedItem != null && usedCount < assignedItem.Quantity)
        {
            usedCount++;
            UpdateUI();
            UI_CharacterEnhancement.Instance.CalculatePreview();
        }
    }

    private void OnClickMinus()
    {
        if (usedCount > 0)
        {
            usedCount--;
            UpdateUI();
            UI_CharacterEnhancement.Instance.CalculatePreview();
        }
    }

    public void SetUsedCount(int count)
    {
        if (assignedItem == null) return;
        usedCount = Mathf.Clamp(count, 0, assignedItem.Quantity);
        UpdateUI();
    }

    public int GetExpYield()
    {
        if (assignedItem == null || assignedItem.ItemData == null) return 0;
        return assignedItem.ItemData.expValue * usedCount;
    }

    public ItemHolder GetItem() => assignedItem;
}
