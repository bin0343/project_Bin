using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Equipment : MonoBehaviour
{
    public static Player_Equipment instance;

    public ItemHolder[] equipmentSlots = new ItemHolder[System.Enum.GetValues(typeof(EquipmentType)).Length];

    // REMOVED: 멤버 변수로 저장하지 않습니다.
    // private Player_Stat playerStat;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        instance = this;
    }

    // REMOVED: Start에서 playerStat을 미리 찾아두지 않습니다.
    // private void Start() { ... }

    public void Equip(ItemHolder itemToEquip, SlotType sourceType, int sourceIndex)
    {
        // CHANGED: 함수가 호출될 때마다 Player_Stat을 직접 찾아옵니다.
        Player_Stat stat = GetComponent<Player_Stat>();
        if (stat == null)
        {
            Debug.LogError("Equip 실패: Player_Stat 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        if (itemToEquip == null || itemToEquip.ItemData.itemType != ITEMTYPE.Equipment) return;

        Item_Equipment equipmentData = itemToEquip.ItemData as Item_Equipment;
        if (equipmentData == null) return;

        int slotIndex = (int)equipmentData.equipmentType;

        ItemHolder previouslyEquipped = UnEquip(equipmentData.equipmentType);

        equipmentSlots[slotIndex] = itemToEquip;

        if (sourceType == SlotType.INVENTORY)
        {
            Player_Inventory.instance.inventorySlots[sourceIndex] = previouslyEquipped;
        }
        else if (sourceType == SlotType.QUICKSLOT)
        {
            Player_Inventory.instance.quickSlots[sourceIndex] = previouslyEquipped;
        }

        // 스탯 적용
        stat.attackPower += equipmentData.attackBonus;
        stat.defensePower += equipmentData.defenseBonus;
        Debug.Log($"{equipmentData.itemName}을(를) 장착했습니다.");

        RefreshUI();
    }

    public ItemHolder UnEquip(EquipmentType slotToUnEquip)
    {
        // CHANGED: 함수가 호출될 때마다 Player_Stat을 직접 찾아옵니다.
        Player_Stat stat = GetComponent<Player_Stat>();
        if (stat == null)
        {
            Debug.LogError("UnEquip 실패: Player_Stat 컴포넌트를 찾을 수 없습니다!");
            return null;
        }

        int slotIndex = (int)slotToUnEquip;
        ItemHolder itemToUnEquip = equipmentSlots[slotIndex];

        if (itemToUnEquip != null)
        {
            Item_Equipment equipmentData = itemToUnEquip.ItemData as Item_Equipment;
            if (equipmentData != null)
            {
                // 스탯 해제
                stat.attackPower -= equipmentData.attackBonus;
                stat.defensePower -= equipmentData.defenseBonus;
            }

            equipmentSlots[slotIndex] = null;
        }
        return itemToUnEquip;
    }

    private void RefreshUI()
    {
        if (UI_Manager.Instance?.UI_Status?.uiEquipmentPanel != null)
        {
            UI_Manager.Instance.UI_Status.uiEquipmentPanel.RefreshUI();
        }
    }

}
