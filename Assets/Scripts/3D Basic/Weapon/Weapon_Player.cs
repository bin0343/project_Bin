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
        if (!GetComponent<Collider>().enabled) return;
        if (hasHit) return;

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
                    // 1. 현재 상태가 러닝 어택 상태일 경우
                    // 예시: AttackType.Stun 또는 새로운 AttackType.RunningAttack 등
                    enemyStat.TakeDamage(damage, AttackType.Knockback);
                    Debug.Log($"[러닝 어택] 몬스터가 {damage} 만큼 피해를 입음");
                }
                if (playerAction.currentComboStep == 3)
                {
                    enemyStat.TakeDamage(damage, AttackType.Knockback);
                }
                else
                {
                    enemyStat.TakeDamage(damage, AttackType.Normal);
                }
                Debug.Log($"몬스터가 {damage} 만큼 피해를 입음");
            }
        }
    }
}
