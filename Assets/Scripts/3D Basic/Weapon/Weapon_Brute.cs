using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Brute : MonoBehaviour
{
    public int WeaponAttackPower = 2;
    public bool HasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (HasHit) return;

        if (other.CompareTag("Player"))
        {
            HasHit = true;

            Player_Stat playerStat = other.GetComponent<Player_Stat>();
            Enemy_Stat enemyStat = GetComponentInParent<Enemy_Stat>();

            if (playerStat != null && enemyStat != null)
            {
                int damage = Mathf.Max(enemyStat.AttackPower + WeaponAttackPower - playerStat.DefensePower, 1);
                playerStat.TakeDamage(damage);
                Debug.Log($"플레이어가 {damage} 만큼 피해를 입음");
            }
        }
    }
}
