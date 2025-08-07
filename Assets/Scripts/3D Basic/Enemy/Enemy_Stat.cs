using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Stat : MonoBehaviour
{
    public string EnemyName;
    public int MaxHP = 80;
    public int CurrentHP = 80;

    public int AttackPower = 8;
    public int DefensePower = 3;

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(CurrentHP, 0);
        Debug.Log("몬스터가 피해를 입음. 남은 체력: " + CurrentHP);
    }
}
