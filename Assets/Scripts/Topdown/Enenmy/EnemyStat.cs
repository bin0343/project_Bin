using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStat : MonoBehaviour
{
    public int MaxHp = 50;
    public int CurrentHp;

    public int Attack = 10;
    public int Defense = 5;

    public int TakeDamage(int attackerAttack)
    {
        int damage = Mathf.Max(attackerAttack - Defense, 1);
        CurrentHp = Mathf.Clamp(CurrentHp - damage, 0, MaxHp);
        return damage;
    }

    public bool IsDead()
    {
        return CurrentHp <= 0;
    }
}
