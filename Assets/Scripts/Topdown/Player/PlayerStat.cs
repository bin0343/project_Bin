using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStat : MonoBehaviour
{
    public int Level = 1;
    public int Exp = 0;
    public int LevelUpExp = 100;
    public int Gold = 100;

    public int MaxHp = 100;
    public int currentHp;
    public int MaxMp = 50;
    public int currentMp;

    public int Attack = 10;
    public int Defense = 5;

    public int StatPoints = 0;

    public int TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(damage - Defense, 1); // 최소 1 보장
        currentHp = Mathf.Clamp(currentHp - finalDamage, 0, MaxHp);
        return finalDamage;
    }

    public void Heal(int amount)
    {
        currentHp = Mathf.Clamp(currentHp + amount, 0, MaxHp);
    }

    public void RecoverMp(int amount)
    {
        currentMp = Mathf.Clamp(currentMp + amount, 0, MaxMp);
    }

    public void GainExp(int amount)
    {
        Exp += amount;
        while(Exp >= LevelUpExp)
        {
            Exp -= LevelUpExp;
            LevelUp();
        }
    }

    public void GainGold(int amount)
    {
        Gold += amount;
    }

    private void LevelUp()
    {
        Level++;
        LevelUpExp *= 2;

        MaxHp += 10;
        MaxMp += 5;
        currentHp = MaxHp;
        currentMp = MaxMp;

        StatPoints += 3;
    }

    //스탯 포인트
    public void IncreaseAttack()
    {
        if (StatPoints > 0)
        {
            Attack++;
            StatPoints--;
        }
    }

    public void DecreaseAttack()
    {
        if (Attack > 1)
        {
            Attack--;
            StatPoints++;
        }
    }

    public void IncreaseDefense()
    {
        if (StatPoints > 0)
        {
            Defense++;
            StatPoints--;
        }
    }

    public void DecreaseDefense()
    {
        if (Defense > 1)
        {
            Defense--;
            StatPoints++;
        }
    }
}
