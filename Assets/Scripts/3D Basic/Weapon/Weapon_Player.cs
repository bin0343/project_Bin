using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Player : MonoBehaviour
{
    [Header("Weapon Components")]
    [SerializeField] private Collider attackCollider;
    [SerializeField] private TrailRenderer slashTrail;

    public int weaponAttackPower = 3;   //아마 없애도 될듯?
    public bool hasHit = false;

    void Awake()
    {
        if (attackCollider == null) attackCollider = GetComponent<Collider>();
        if (slashTrail == null) slashTrail = GetComponentInChildren<TrailRenderer>();
        GetComponentInChildren<Collider>().enabled = false;
        DisableHitbox();
        StopTrail();
    }

    public void EnableHitbox()
    {
        if (attackCollider != null) attackCollider.enabled = true;
        hasHit = false;
    }

    public void DisableHitbox()
    {
        if (attackCollider != null) attackCollider.enabled = false;
    }

    public void StartTrail()
    {
        if (slashTrail != null) slashTrail.emitting = true;
    }

    public void StopTrail()
    {
        if (slashTrail != null) slashTrail.emitting = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attackCollider.enabled || hasHit) return;
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
            //EnemyBase enemyBase = other.GetComponent<EnemyBase>();
            Player_Stat PlayerStat = GetComponentInParent<Player_Stat>();
            Player_Action playerAction = GetComponentInParent<Player_Action>();     //비동기, 유니테스크

            if (PlayerStat != null && playerAction != null)
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

                enemyStat.TakeDamage(damage, currentAttackType);

                /*switch (currentAttackType)
                {
                    case AttackType.Normal:
                        enemyBase.EnterStunState(1.5f);
                        break;
                    case AttackType.Knockback:
                        enemyBase.EnterStunState(1.5f);
                        StartCoroutine(enemyBase.ApplyKnockback());
                        break;
                }*/
            }
        }
    }
}
