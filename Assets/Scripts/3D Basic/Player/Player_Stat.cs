using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Stat : MonoBehaviour
{
    public int MaxHP = 100;
    public int CurrentHP = 100;

    public int AttackPower = 10;
    public int DefensePower = 5;

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(CurrentHP, 0);
        Debug.Log("플레이어가 피해를 입음. 남은 체력: " + CurrentHP);
    }
}
