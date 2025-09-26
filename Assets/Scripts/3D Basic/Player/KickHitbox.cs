using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickHitbox : MonoBehaviour
{
    public int KickAttackPower = 1;
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

            Enemy_Stat EnemyStat = other.GetComponent<Enemy_Stat>();
            Player_Stat PlayerStat = GetComponentInParent<Player_Stat>();

            if (EnemyStat != null && PlayerStat != null)
            {
                int damage = Mathf.Max(KickAttackPower - EnemyStat.defensePower, 1);
                EnemyStat.TakeDamage(damage, AttackType.Normal);
                Debug.Log($"몬스터가 {damage} 만큼 피해를 입음");
            }
        }
    }
}
