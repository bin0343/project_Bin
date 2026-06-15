using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemHolder
{
    public Item_Base ItemData;// 아이템 원본 데이터
    public int Quantity;                            // 아이템 개수
    private float LastUseTime;

    [Header("무기 전용 성장 데이터")]
    public int weaponLevel = 1;
    public int weaponExp = 0;
    public int breakthroughStage = 0;   //0: 최대 20, 1: 최대 40, 2: 최대 50
    public int refinementStage = 1;     //1~5 (합성단계)

    public ItemHolder(Item_Base itemData, int quantity)
    {
        ItemData = itemData;
        this.Quantity = quantity;

        if (ItemData != null && ItemData.itemType == ITEMTYPE.Equipment)
        {
            this.Quantity = 1;
            ItemData.isStackable = false;
        }
    }

    public void AddQuantity(int amount)
    {
        Quantity += amount;
    }

    public bool Use(GameObject user)
    {
        if (ItemData == null) return false;
        return ItemData.Use(user);
    }

    public float GetRemainingCooldown()
    {
        return 0f;
    }

    //무기 성장 로직
    public int GetMaxLevel()
    {
        if (breakthroughStage == 0) return 20;
        if (breakthroughStage == 1) return 40;
        return 50;
    }

    public int GetRequireExpForNextLevel()
    {
        if (weaponLevel < 1) weaponLevel = 1;

        return weaponLevel * 100;
    }

    // 무기 누적 경험치 계산
    public int GetTotalAccumulatedExp()
    {
        if (ItemData == null || ItemData.itemType != ITEMTYPE.Equipment) return 0;

        int totalExp = 0;
        for (int i = 1; i < weaponLevel; i++)
        {
            totalExp += i * 100;
        }
        totalExp += weaponExp;

        return totalExp;
    }

    // 현재 레벨과 돌파, 합성 상태가 모두 적용된 최종 공격력 계산
    public int GetTotalWeaponAttack()
    {
        if (weaponLevel < 1) weaponLevel = 1;
        if (refinementStage < 1) refinementStage = 1;

        if (ItemData is Item_Equipment eq)
        {
            // 공식: (기본공 + 레벨보너스) * 합성보너스
            float levelBonus = (weaponLevel - 1) * eq.attackGrowthPerLevel;
            float refineMultiplier = 1f + ((refinementStage - 1) * eq.refinementBonusPercent);

            float finalAttack = (eq.attackBonus + levelBonus) * refineMultiplier;
            return Mathf.RoundToInt(finalAttack);
        }
        return 0;
    }

    public void AddWeaponExp(int expAmount)
    {
        if (ItemData is not Item_Equipment eq) return;

        int maxLevel = GetMaxLevel();
        if (weaponLevel >= maxLevel) return;

        weaponExp += expAmount;

        while (weaponExp >= GetRequireExpForNextLevel() && weaponLevel < maxLevel)
        {
            weaponExp -= GetRequireExpForNextLevel();
            weaponLevel++;
        }

        if (weaponLevel >= maxLevel)
        {
            weaponLevel = maxLevel;
            weaponExp = 0;
        }
    }

    public bool TryBreakthrough()
    {
        if (weaponLevel < GetMaxLevel()) return false;
        if (breakthroughStage >= 2) return false;

        breakthroughStage++;
        return true;
    }

    public bool TryRefine()
    {
        if (refinementStage >= 5) return false;
        refinementStage++;
        return true;
    }
}
