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

            Player_Stat playerStat = other.GetComponent<Player_Stat>();
            NPC_Stat npcStat = other.GetComponent<NPC_Stat>();
            Enemy_Stat enemyStat = GetComponentInParent<Enemy_Stat>();

            // 몬스터 스탯이 없으면 기본 공격력 10으로 설정 (안전장치)
            int attackerPower = (enemyStat != null) ? enemyStat.attackPower : 10;

            // [플레이어 피격]
            if (playerStat != null)
            {
                int damage = Mathf.Max(attackerPower + weaponAttackPower - playerStat.defensePower, 1);
                playerStat.TakeDamage(damage);
                Debug.Log($"플레이어 피격! (데미지: {damage}, 남은체력: {playerStat.currentHP})");
            }
            // [동료(NPC) 피격]
            else if (npcStat != null)
            {
                int npcdamage = Mathf.Max(attackerPower + weaponAttackPower - npcStat.defensePower, 1);

                // 로그로 체력 확인 (이걸로 한 방에 죽는지 확인하세요)
                Debug.Log($"동료 피격 전 체력: {npcStat.currentHP} / 데미지: {npcdamage}");

                // [중요] 공격자(enemyBase.transform)를 넘겨줘야 NPC가 반격합니다!
                Transform attackerTransform = (enemyBase != null) ? enemyBase.transform : this.transform;
                npcStat.TakeDamage(npcdamage, attackerTransform);
            }
        }
    }
}