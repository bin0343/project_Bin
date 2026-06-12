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
    public int attackBonus;
    public WeaponCategory weaponCategory = WeaponCategory.OneHandedSword;   //무기 종류

    [Header("장착 시 외형")]
    [Tooltip("이 장비를 착용했을 때 캐릭터 손에 생성될 무기 프리팹")]
    public GameObject weaponPrefab;

    public override bool Use(GameObject user)
    {
        bool itemUsed = false;
        
        return itemUsed;
    }
}
