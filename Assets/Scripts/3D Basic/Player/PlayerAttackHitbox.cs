using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField]
    private Collider AttackHitbox;
    [SerializeField]
    private Collider KickHitbox;

    private bool HasHit = false;
    Weapon_Player Weapon_Player;

    private void Awake()
    {
        Weapon_Player = GetComponentInChildren<Weapon_Player>();
        if (AttackHitbox != null)
            AttackHitbox.enabled = false;

        if (KickHitbox != null)
            KickHitbox.enabled = false;
    }

    public void EnableAttackHitbox()
    {
        AttackHitbox.enabled = true;
        HasHit = false;
    }

    public void DisableAttackHitbox()
    {
        AttackHitbox.enabled = false;
    }

    public void ResetHasHit()
    {
        Weapon_Player.HasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HasHit) return;

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("공격!");
            HasHit = true;
        }
    }

    public void EnableKickHitbox()
    {
        KickHitbox.enabled = true;

        KickHitbox.GetComponentInChildren<KickHitbox>().HasHit = false; // 중복 타격 방지 초기화
    }

    public void DisableKickHitbox()
    {
        KickHitbox.enabled = false;
    }
}
