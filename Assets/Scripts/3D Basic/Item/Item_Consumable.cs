using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Item/Consumable")]
public class Item_Consumable : Item_Base
{
    public int HealAmount;
    public int RecoverMpAmount;

    public override void Use(GameObject user)
    {
        var stat = user.GetComponent<Player_Stat>();

        if (stat.CurrentHP >= stat.MaxHP)
        {
            Debug.Log($"{itemName}을(를) 사용할 수 없습니다. 이미 최대 체력입니다.");
            return;
        }

        if (stat.CurrentMP >= stat.MaxMP)
        {
            Debug.Log($"{itemName}을(를) 사용할 수 없습니다. 이미 최대 마나입니다.");
            return;
        }

        if (stat != null)
        {
            //stat.CurrentHP += HealAmount;
            stat.CurrentHP = Mathf.Min(stat.CurrentHP + HealAmount, stat.MaxHP);
            Debug.Log($"{itemName}을(를) 사용하여 체력을 {HealAmount} 회복했습니다.");
        }
    }
}
