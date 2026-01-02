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

    private void Start()
    {
        InitializeEquipment();
    }

    private void InitializeEquipment()
    {
        Player_Stat stat = GetComponent<Player_Stat>();

        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            // 슬롯에 아이템이 있고, 데이터가 유효하다면
            if (equipmentSlots[i] != null && equipmentSlots[i].ItemData != null)
            {
                Item_Equipment equipmentData = equipmentSlots[i].ItemData as Item_Equipment;
                if (equipmentData == null) continue;

                // 1. 무기 프리팹 생성 (무기 슬롯인 경우 혹은 무기 프리팹이 있는 경우)
                if (equipmentData.weaponPrefab != null)
                {
                    // 기존 무기가 있다면 제거 (혹시 모를 중복 방지)
                    if (currentWeaponObject != null) Destroy(currentWeaponObject);

                    currentWeaponObject = Instantiate(equipmentData.weaponPrefab, weaponMountPoint);
                    Weapon_Player newWeaponController = currentWeaponObject.GetComponentInChildren<Weapon_Player>();
                    playerAction.SetCurrentWeapon(newWeaponController);
                }

                // 2. 애니메이션 오버라이드 적용
                if (equipmentData.animationOverrides != null)
                {
                    animator.runtimeAnimatorController = equipmentData.animationOverrides;
                }

                // 3. 스탯 적용
                if (stat != null)
                {
                    stat.AddEquipmentStat(STAT.Attack, equipmentData.attackBonus);
                    stat.AddEquipmentStat(STAT.Defense, equipmentData.defenseBonus);
                }
            }
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
