using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Item/Equipment")]
public class Item_Equipment : Item_Base
{
    [Header("장비 정보")]
    public EquipmentType equipmentType;
    public int attackBonus;
    public int defenseBonus;

    [Header("장착 시 외형")]
    [Tooltip("이 장비를 착용했을 때 캐릭터 손에 생성될 무기 프리팹입니다. 무기가 아니라면 비워두세요.")]
    public GameObject weaponPrefab;

    public override bool Use(GameObject user)
    {
        bool itemUsed = false;
        
        return itemUsed;
    }
}
