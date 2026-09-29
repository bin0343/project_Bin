using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemManager : MonoBehaviour
{
    public static UI_ItemManager instance { get; private set; }

    [Header("º“∫Ò æ∆¿Ã≈€ ƒ¸ΩΩ∑‘")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text hotkeyText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        if (hotkeyText != null)
        {
            hotkeyText.text = "R";
        }

        RefreshQuickSlot();
    }

    public void RefreshQuickSlot()
    {
        if (Player_Inventory.instance == null)
        {
            ClearQuickSlot();
            return;
        }

        Item_Base item = Player_Inventory.instance.quickSlotItem;
        int quantity = Player_Inventory.instance.GetQuickSlotQuantity();

        if (item == null || quantity <= 0)
        {
            ClearQuickSlot();
            return;
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = item.itemIcon;
            itemIcon.gameObject.SetActive(true);
        }

        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
    }

    private void ClearQuickSlot()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.gameObject.SetActive(false);
        }

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }
    }
}