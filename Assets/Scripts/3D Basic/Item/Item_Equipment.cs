using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponCategory
{
    OneHandedSword, // 한손검
    Greatsword,     // 대검
    Arrow         // 권총
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Item/Equipment")]
public class Item_Equipment : Item_Base
{
    [Header("장비 정보")]
    public EquipmentType equipmentType = EquipmentType.Weapon;
    public WeaponCategory weaponCategory = WeaponCategory.OneHandedSword;   //무기 종류
    public int attackBonus;

    [Header("성장 능력치")]
    [Tooltip("1레벨 오를 때마다 증가하는 공격력")]
    public float attackGrowthPerLevel = 5.0f;
    [Tooltip("동일 무기 1번 합성할 때마다 증가하는 공격력 비율 (0.1 = 10% 증가)")]
    public float refinementBonusPercent = 0.1f;

    [Header("장착 시 외형")]
    [Tooltip("이 장비를 착용했을 때 캐릭터 손에 생성될 무기 프리팹")]
    public GameObject weaponPrefab;

    private void OnValidate()
    {
        isStackable = false;
        maxStackSize = 1;
        itemType = ITEMTYPE.Equipment;
    }

    public override bool Use(GameObject user)
    {
        return false;
    }
}
