using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Equipment : MonoBehaviour
{
    public static Player_Equipment Instance;
    public ItemHolder[] equipmentSlots = new ItemHolder[System.Enum.GetValues(typeof(EquipmentType)).Length];
    private Player_Stat playerStat;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        playerStat = GetComponent<Player_Stat>();
    }

    public void Equip(ItemHolder itemToEquip, int sourceInventoryIndex)
    {
        if (itemToEquip == null || itemToEquip.ItemData.itemType != ITEMTYPE.Equipment) return;

        Item_Equipment equipmentData = itemToEquip.ItemData as Item_Equipment;
        int slotIndex = (int)equipmentData.equipmentType;

        // 1. 기존에 착용하고 있던 아이템이 있다면 해제하고 인벤토리로 보냄
        ItemHolder previouslyEquipped = UnEquip(equipmentData.equipmentType);

        // 2. 새로운 아이템을 장착
        equipmentSlots[slotIndex] = itemToEquip;
        Player_Inventory.Instance.inventorySlots[sourceInventoryIndex] = previouslyEquipped; // 빈 자리에 이전 아이템 넣기

        // 3. 스탯 적용
        playerStat.AttackPower += equipmentData.attackBonus;
        playerStat.DefensePower += equipmentData.defenseBonus;

        Debug.Log($"{equipmentData.itemName}을(를) 장착했습니다.");
        RefreshUI();
    }

    public ItemHolder UnEquip(EquipmentType slotToUnEquip)
    {
        int slotIndex = (int)slotToUnEquip;
        ItemHolder itemToUnEquip = equipmentSlots[slotIndex];

        if (itemToUnEquip != null)
        {
            Item_Equipment equipmentData = itemToUnEquip.ItemData as Item_Equipment;

            // 스탯 해제
            playerStat.AttackPower -= equipmentData.attackBonus;
            playerStat.DefensePower -= equipmentData.defenseBonus;

            equipmentSlots[slotIndex] = null;
            Debug.Log($"{equipmentData.itemName}을(를) 해제했습니다.");
            RefreshUI();
        }
        return itemToUnEquip;
    }

    private void RefreshUI()
    {
        // UI_EquipmentPanel이 있다면 새로고침 호출 (4단계에서 생성)
        /*if (UI_Manager.Instance.UI_Status.uiEquipmentPanel != null)
        {
            UI_Manager.Instance.UI_Status.uiEquipmentPanel.RefreshUI();
        }*/
    }

}
