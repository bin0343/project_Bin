using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponSelectionSlot : MonoBehaviour
{
    public Image weaponIcon;
    //public Text weaponNameText;

    private ItemHolder itemHolder;
    private int inventoryIndex; // 인벤토리의 몇 번째 칸에 있던 무기인지 기억
    private UI_WeaponTab parentWeaponTab;

    public void Setup(ItemHolder holder, int index, UI_WeaponTab weaponTab)
    {
        itemHolder = holder;
        inventoryIndex = index;
        parentWeaponTab = weaponTab;

        if (holder.ItemData != null)
        {
            weaponIcon.sprite = holder.ItemData.itemIcon;
            //weaponNameText.text = holder.ItemData.itemName;
        }
    }

    public void OnClickSlot()
    {
        if (parentWeaponTab != null && itemHolder != null)
        {
            parentWeaponTab.ShowPreview(itemHolder, inventoryIndex);
        }
    }
}