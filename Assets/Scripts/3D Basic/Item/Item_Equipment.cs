using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Item/Equipment")]
public class Item_Equipment : Item_Base
{
    public int AttackBouns;
    public int DefenseBouns;
    public override void Use(GameObject user)
    {
        /*var equipmentManager = user.GetComponent<Player_EquipmentManager>();
        if (equipmentManager != null)
        {
            equipmentManager.Equip(this);
            Debug.Log($"{itemName}을(를) 장착했습니다.");
        }*/
    }
}
