using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Equipment : MonoBehaviour
{
    public static Player_Equipment instance;
    private Player_Action playerAction;
    private Animator animator;
    private RuntimeAnimatorController defaultAnimatorController;

    [Header("Equipment Setup")]
    [SerializeField] private Transform weaponMountPoint;
    private GameObject currentWeaponObject;

    public ItemHolder[] equipmentSlots = new ItemHolder[System.Enum.GetValues(typeof(EquipmentType)).Length];

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        instance = this;
        playerAction = GetComponent<Player_Action>();
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            defaultAnimatorController = animator.runtimeAnimatorController;
        }
    }

    public void Equip(ItemHolder itemToEquip, SlotType sourceType, int sourceIndex)
    {
        if (itemToEquip == null || itemToEquip.ItemData.itemType != ITEMTYPE.Equipment) return;

        Item_Equipment equipmentData = itemToEquip.ItemData as Item_Equipment;
        if (equipmentData == null) return;

        ItemHolder previouslyEquipped = UnEquip(equipmentData.equipmentType);
        int slotIndex = (int)equipmentData.equipmentType;

        equipmentSlots[slotIndex] = itemToEquip;

        if (sourceType == SlotType.INVENTORY)
        {
            Player_Inventory.instance.inventorySlots[sourceIndex] = previouslyEquipped;
        }
        else if (sourceType == SlotType.QUICKSLOT)
        {
            Player_Inventory.instance.quickSlots[sourceIndex] = previouslyEquipped;
        }

        if (equipmentData.weaponPrefab != null)
        {
            currentWeaponObject = Instantiate(equipmentData.weaponPrefab, weaponMountPoint);

            Weapon_Player newWeaponController = currentWeaponObject.GetComponentInChildren<Weapon_Player>();

            playerAction.SetCurrentWeapon(newWeaponController);
        }

        if (equipmentData.animationOverrides != null)
        {
            animator.runtimeAnimatorController = equipmentData.animationOverrides;
        }
        else
        {
            animator.runtimeAnimatorController = defaultAnimatorController;
        }

        // 스탯 적용
        Player_Stat stat = GetComponent<Player_Stat>();
        stat.AddEquipmentStat(STAT.Attack, equipmentData.attackBonus);
        stat.AddEquipmentStat(STAT.Defense, equipmentData.defenseBonus);
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
                stat.RemoveEquipmentStat(STAT.Attack, equipmentData.attackBonus);
                stat.RemoveEquipmentStat(STAT.Defense, equipmentData.defenseBonus);
            }

            equipmentSlots[slotIndex] = null;
        }

        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
            currentWeaponObject = null;
        }

        playerAction.SetCurrentWeapon(null);

        if (animator != null)
        {
            animator.runtimeAnimatorController = defaultAnimatorController;
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
