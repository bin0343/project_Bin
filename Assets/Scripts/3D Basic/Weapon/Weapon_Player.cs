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

        if (other.TryGetComponent<Enemy_Stat>(out Enemy_Stat enemyStat))
        {
            hasHit = true;

            //Enemy_Stat enemyStat = other.GetComponent<Enemy_Stat>();
            EnemyBase enemyBase = other.GetComponent<EnemyBase>();
            Player_Stat PlayerStat = GetComponentInParent<Player_Stat>();
            Player_Action playerAction = GetComponentInParent<Player_Action>();     //비동기, 유니테스크

            if (enemyStat != null && PlayerStat != null && playerAction != null && enemyBase != null)
            {
                int damage = Mathf.Max(PlayerStat.attackPower + weaponAttackPower - enemyStat.defensePower, 1);
                AttackType currentAttackType;
                if (playerAction.currentState is PlayerRunningAttackState || playerAction.currentComboStep == 3)
                {
                    currentAttackType = AttackType.Knockback;
                }
                else
                {
                    currentAttackType = AttackType.Normal;
                }

                // 3. Enemy_Stat에는 데미지만 전달하여 HP를 깎게 함
                enemyStat.TakeDamage(damage, currentAttackType);

                // 4. Weapon_Player가 직접 EnemyBase의 효과 함수를 호출 (핵심 변경점)
                switch (currentAttackType)
                {
                    case AttackType.Normal:
                        enemyBase.EnterStunState(1.5f);
                        break;
                    case AttackType.Knockback:
                        enemyBase.EnterStunState(1.5f);
                        StartCoroutine(enemyBase.ApplyKnockback());
                        break;
                }
            }
        }
    }
}
