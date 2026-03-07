using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Item/Consumable")]
public class Item_Consumable : Item_Base
{
    public int healAmount;
    public int recoverMpAmount;

    public override bool Use(GameObject user)
    {
        var stat = user.GetComponent<Player_Stat>();
        if (stat == null) return false;

        bool itemUsed = false; // 아이템이 실제로 효과를 발휘했는지 추적

        if (healAmount > 0 && stat.currentHP < stat.maxHP)
        {
            stat.currentHP = Mathf.Min(stat.currentHP + healAmount, stat.maxHP);
            Debug.Log($"{itemName}을(를) 사용하여 체력을 {healAmount} 회복했습니다.");
            itemUsed = true;
        }

        if (!itemUsed)
        {
            Debug.Log($"{itemName}을(를) 사용했지만 아무 효과가 없었습니다.");
        }

        return itemUsed; // 성공 여부 반환
    }
}
