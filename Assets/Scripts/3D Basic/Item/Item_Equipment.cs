using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Item/Equipment")]
public class Item_Equipment : Item_Base
{
    public EquipmentType equipmentType;
    public int attackBonus;
    public int defenseBonus;
    public override bool Use(GameObject user)
    {
        bool itemUsed = false;
        
        return itemUsed;
    }
}
