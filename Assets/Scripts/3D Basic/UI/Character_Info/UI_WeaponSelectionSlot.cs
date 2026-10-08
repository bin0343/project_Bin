using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponSelectionSlot : MonoBehaviour
{
    public Image weaponIcon;
    public TMP_Text refinementText;

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

            if (refinementText != null)
            {
                bool isRefined = holder.refinementStage > 1;

                refinementText.gameObject.SetActive(isRefined);

                if (isRefined)
                {
                    refinementText.text = $"+{holder.refinementStage - 1}";
                }
            }
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