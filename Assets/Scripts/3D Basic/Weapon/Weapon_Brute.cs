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
        if (other.CompareTag("Player"))
        {
            hasHit = true;

            Player_Stat playerStat = other.GetComponent<Player_Stat>();
            Enemy_Stat enemyStat = GetComponentInParent<Enemy_Stat>();

            if (playerStat != null && enemyStat != null)
            {
                int damage = Mathf.Max(enemyStat.AttackPower + weaponAttackPower - playerStat.defensePower, 1);
                playerStat.TakeDamage(damage);
                Debug.Log($"플레이어가 {damage} 만큼 피해를 입음");
            }
        }
    }
}
