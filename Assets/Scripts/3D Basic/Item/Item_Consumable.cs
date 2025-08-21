using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Item/Consumable")]
public class Item_Consumable : Item_Base
{
    public int HealAmount;

    public override void Use(GameObject user)
    {
        var stat = user.GetComponent<Player_Stat>();
        if (stat != null)
        {
            stat.CurrentHP += HealAmount;
            Debug.Log($"{itemName}을(를) 사용하여 체력을 {HealAmount} 회복했습니다.");
        }
    }
}
