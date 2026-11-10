using UnityEngine;

public static class CombatDamageCalculator
{
    // 방어력 효율을 결정하는 기준값
    private const float DefenseConstant = 100f;

    public static int CalculateFinalDamage(int attack, int defense)
    {
        // 공격력 자체가 없다면 데미지도 없음
        if (attack <= 0)
            return 0;

        // 방어력이 음수가 되지 않도록 보정
        defense = Mathf.Max(defense, 0);

        // 방어력에 따른 최종 데미지 배율
        float defenseMultiplier = DefenseConstant / (DefenseConstant + defense);

        // 공격력 × 방어 배율
        float calculatedDamage = attack * defenseMultiplier;

        // 정수로 반올림하고 최소 1 데미지 보장
        int finalDamage = Mathf.Max(Mathf.RoundToInt(calculatedDamage), 1);

        return finalDamage;
    }
}