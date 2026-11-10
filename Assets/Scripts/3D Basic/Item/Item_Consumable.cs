
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Item/Consumable")]
public class Item_Consumable : Item_Base
{
    [Header("체력 회복 설정")]
    [Range(0f, 100f)]
    public float healPercent = 30f;

    public override bool Use(GameObject user)
    {
        Character_Stat stat = user.GetComponent<Character_Stat>();
        if (stat == null) return false;

        bool itemUsed = false;

        if (healPercent > 0f && stat.maxHP > 0)
        {
            int healAmount = Mathf.CeilToInt(stat.maxHP * (healPercent / 100f));

            int previousHP = stat.currentHP;

            if (stat.Heal(healAmount))
            {
                int actualHeal = stat.currentHP - previousHP;

                Debug.Log($"{itemName}을(를) 사용하여 체력을 " + $"{actualHeal} 회복했습니다. ({healPercent}%)");

                itemUsed = true;
            }
        }

        if (!itemUsed)
        {
            Debug.Log($"{itemName}을(를) 사용했지만 아무 효과가 없었습니다.");
        }

        return itemUsed;
    }
}
