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

            // 부모(EnemyBase)에게 공격이 막혔다고 알림
            if (enemyBase != null)
            {
                enemyBase.OnAttackParried();
            }

            // 플레이어의 방패에게 방어 성공 리액션을 하라고 알림
            shield.OnParrySuccess();

            // 방패에 막혔으므로 데미지 로직은 실행하지 않고 종료
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
