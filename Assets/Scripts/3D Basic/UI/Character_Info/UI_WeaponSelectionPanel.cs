using UnityEngine;
using System.Collections.Generic;

public class UI_WeaponSelectionPanel : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject slotPrefab;
    public Transform contentParent;

    public UI_WeaponTab parentWeaponTab;

    private void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        Player_Equipment activeEquip = null;
        if (BattleManager.instance != null)
        {
            GameObject activePlayer = BattleManager.instance.GetActiveCharacter();
            if (activePlayer != null) activeEquip = activePlayer.GetComponent<Player_Equipment>();
        }

        if (Player_Inventory.instance == null || activeEquip == null) return;

        List<ItemHolder> inv = Player_Inventory.instance.inventorySlots;
        WeaponCategory currentUsableCategory = activeEquip.usableWeaponCategory;

        for (int i = 0; i < inv.Count; i++)
        {
            ItemHolder holder = inv[i];

            if (holder != null && holder.ItemData != null && holder.ItemData.itemType == ITEMTYPE.Equipment)
            {
                Item_Equipment eqData = holder.ItemData as Item_Equipment;

                if (eqData != null && eqData.weaponCategory == currentUsableCategory)
                {
                    // 프리팹 생성 및 데이터 세팅
                    GameObject go = Instantiate(slotPrefab, contentParent);
                    UI_WeaponSelectionSlot slotScript = go.GetComponent<UI_WeaponSelectionSlot>();

                    if (slotScript != null)
                    {
                        slotScript.Setup(holder, i, parentWeaponTab);

                        UnityEngine.UI.Button btn = go.GetComponent<UnityEngine.UI.Button>();
                        if (btn != null)
                        {
                            btn.onClick.AddListener(slotScript.OnClickSlot);
                        }
                    }
                }
            }
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}