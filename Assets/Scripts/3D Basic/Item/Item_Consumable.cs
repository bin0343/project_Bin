using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Item/Consumable")]
public class Item_Consumable : Item_Base
{
    public int HealAmount;
    public int RecoverMpAmount;

    public override bool Use(GameObject user)
    {
        var stat = user.GetComponent<Player_Stat>();
        if (stat == null) return false;

        bool itemUsed = false; // 아이템이 실제로 효과를 발휘했는지 추적

        // 체력 회복 로직
        if (HealAmount > 0 && stat.CurrentHP < stat.MaxHP)
        {
            stat.CurrentHP = Mathf.Min(stat.CurrentHP + HealAmount, stat.MaxHP);
            Debug.Log($"{itemName}을(를) 사용하여 체력을 {HealAmount} 회복했습니다.");
            itemUsed = true;
        }

        // 마나 회복 로직
        if (RecoverMpAmount > 0 && stat.CurrentMP < stat.MaxMP)
        {
            stat.CurrentMP = Mathf.Min(stat.CurrentMP + RecoverMpAmount, stat.MaxMP);
            Debug.Log($"{itemName}을(를) 사용하여 마나를 {RecoverMpAmount} 회복했습니다.");
            itemUsed = true;
        }

        // 만약 체력과 마나가 모두 가득 차 있어서 아무 효과가 없었다면
        if (!itemUsed)
        {
            Debug.Log($"{itemName}을(를) 사용했지만 아무 효과가 없었습니다.");
        }

        return itemUsed; // 성공 여부 반환
    }
}
