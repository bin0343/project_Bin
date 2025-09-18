using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Player : MonoBehaviour
{
    public int WeaponAttackPower = 3;
    public bool HasHit = false;

    void Awake()
    {
        GetComponent<Collider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GetComponent<Collider>().enabled) return;
        if (HasHit) return;

        if (other.CompareTag("Enemy"))
        {
            HasHit = true;

            Enemy_Stat enemyStat = other.GetComponent<Enemy_Stat>();
            Player_Stat PlayerStat = GetComponentInParent<Player_Stat>();

            if (enemyStat != null && PlayerStat != null)
            {
                int damage = Mathf.Max(PlayerStat.AttackPower + WeaponAttackPower - enemyStat.DefensePower, 1);
                if (PlayerAttackState.comboStep == 3)
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
