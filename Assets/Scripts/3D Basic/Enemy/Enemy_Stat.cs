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
        int prevHP = CurrentHP;
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(CurrentHP, 0);

        UI_MonsterUIManager uiManager = FindObjectOfType<UI_MonsterUIManager>();
        if (uiManager != null)
        {
            // 체력만 업데이트, 타겟 변경하지 않음
            uiManager.UpdateHPBar(prevHP, CurrentHP, MaxHP);
        }

        Debug.Log("몬스터가 피해를 입음. 남은 체력: " + CurrentHP);
    }
}
