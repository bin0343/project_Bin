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
            Player_Action playerAction = GetComponentInParent<Player_Action>();
            bool isGuardBreak = (playerAction.currentState is PlayerRunningAttackState);

            if (isGuardBreak)
            {
                EnemyBase enemyBase = other.GetComponentInParent<EnemyBase>();
                if (enemyBase != null)
                {
                    // 2. 넉백과 함께 스턴 상태로 만듦
                    //enemyBase.EnterStunState(1.5f);
                    enemyBase.StartCoroutine(enemyBase.ApplyKnockback());
                }
            }
            else
            {
                // [일반 공격일 때] -> 그냥 막힘 (기존 로직)
                Debug.Log("플레이어: 공격이 몬스터의 무기에 막혔다!");
                playerAction?.OnAttackBlocked();
                enemyDefense.OnParrySuccess();
            }
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
