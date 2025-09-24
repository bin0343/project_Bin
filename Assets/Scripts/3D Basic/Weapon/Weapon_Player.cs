using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Player : MonoBehaviour
{
    public int weaponAttackPower = 3;
    public bool hasHit = false;

    void Awake()
    {
        GetComponent<Collider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GetComponent<Collider>().enabled || hasHit) return;
        //if (hasHit) return;

        Weapon_EnemyDefense enemyDefense = other.GetComponent<Weapon_EnemyDefense>();
        if (enemyDefense != null)
        {
            hasHit = true;
            Debug.Log("플레이어: 공격이 몬스터의 무기에 막혔다!");

            GetComponentInParent<Player_Action>()?.OnAttackBlocked();

            // 몬스터에게 방어 성공 리액션을 하라고 알림
            enemyDefense.OnParrySuccess();

            // (선택) 플레이어도 공격이 튕기는 리액션을 할 수 있음
            // GetComponentInParent<Player_Action>()?.PlayAttackBlockedReaction();

            // 막혔으므로 데미지 로직을 실행하지 않고 함수 종료
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            hasHit = true;

            Enemy_Stat enemyStat = other.GetComponent<Enemy_Stat>();
            Player_Stat PlayerStat = GetComponentInParent<Player_Stat>();
            Player_Action playerAction = GetComponentInParent<Player_Action>();     //비동기, 유니테스크

            if (enemyStat != null && PlayerStat != null && playerAction != null)
            {
                int damage = Mathf.Max(PlayerStat.attackPower + weaponAttackPower - enemyStat.DefensePower, 1);
                if (playerAction.currentState is PlayerRunningAttackState)
                {
                    // 1. 러닝 어택 처리
                    enemyStat.TakeDamage(damage, AttackType.Knockback);
                    Debug.Log($"[러닝 어택] 몬스터가 {damage} 만큼 피해를 입음");
                }
                else if (playerAction.currentComboStep == 3)
                {
                    // 2. 콤보 3타 처리
                    enemyStat.TakeDamage(damage, AttackType.Knockback);
                    Debug.Log($"[콤보 3타] 몬스터가 {damage} 만큼 피해를 입음");
                }
                else
                {
                    // 3. 일반 공격(콤보 1, 2타 등) 처리
                    enemyStat.TakeDamage(damage, AttackType.Normal);
                    Debug.Log($"[일반 공격] 몬스터가 {damage} 만큼 피해를 입음");
                }
            }
        }
    }
}
