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

    public int ExpReward = 50;
    public MonsterHpBar hpBar;

    void Start()
    {
        // 자식 오브젝트에서 MonsterHpBar 자동으로 찾아옴
        hpBar = GetComponentInChildren<MonsterHpBar>();
        if (hpBar != null)
            hpBar.Setup(this); // 체력바에 Enemy_Stat 정보 전달
    }

    public void TakeDamage(int damage)
    {
        int prevHP = CurrentHP;
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(CurrentHP, 0);

        if (hpBar != null)
            hpBar.UpdateHpBar();
    }
}
