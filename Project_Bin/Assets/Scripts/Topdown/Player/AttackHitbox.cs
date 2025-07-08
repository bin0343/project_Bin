using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private PlayerStat PlayerStat;

    private void Start()
    {
        PlayerStat = GetComponentInParent<PlayerStat>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<GreenSlimeControl>();
            if (enemy != null && PlayerStat != null)
            {
                enemy.TakeDamage(PlayerStat.Attack, transform.position);
            }
        }
    }
}
