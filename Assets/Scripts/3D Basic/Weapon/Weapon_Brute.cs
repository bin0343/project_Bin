using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Brute : MonoBehaviour
{
    public int weaponAttackPower = 2;
    public bool hasHit = false;
    private EnemyBase enemyBase;

    private void Awake()
    {
        enemyBase = GetComponentInParent<EnemyBase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // 1. 방패 방어 로직
        Shield_Player shield = other.GetComponent<Shield_Player>();
        if (shield != null)
        {
            hasHit = true;
            Debug.Log("몬스터: 공격이 방패에 막혔다!");

            if (enemyBase != null)
            {
                enemyBase.OnAttackParried();
            }

            shield.OnParrySuccess();
            return;
        }

        // 2. 플레이어 또는 동료 피격 로직
        if (other.CompareTag("Player") || other.CompareTag("Companion"))
        {
            hasHit = true;

            Character_Stat targetStat = other.GetComponent<Character_Stat>();
            Enemy_Stat enemyStat = GetComponentInParent<Enemy_Stat>();

            // 몬스터 스탯이 없으면 기본 공격력 10으로 설정 (안전장치)
            int attackerPower = (enemyStat != null) ? enemyStat.attackPower : 10;

            // [플레이어 피격]
            if (targetStat != null)
            {
                int damage = Mathf.Max(attackerPower + weaponAttackPower - targetStat.defensePower, 1);

                targetStat.TakeDamage(damage);

                Debug.Log($"아군 피격! (데미지: {damage}, 남은체력: {targetStat.currentHP})");
            }
        }
    }
}