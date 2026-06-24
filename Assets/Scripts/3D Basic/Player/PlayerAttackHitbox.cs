using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField]
    private Collider kickHitbox;

    private bool hasHit = false;
    Weapon_Player weapon_Player;

    private void Awake()
    {
        weapon_Player = GetComponentInChildren<Weapon_Player>();

        if (kickHitbox != null)
            kickHitbox.enabled = false;
    }

    public void ResetHasHit()
    {
        //weapon_Player.hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("공격!");
            hasHit = true;
        }
    }

    public void EnableKickHitbox()
    {
        kickHitbox.enabled = true;

        kickHitbox.GetComponentInChildren<KickHitbox>().HasHit = false; // 중복 타격 방지 초기화
    }

    public void DisableKickHitbox()
    {
        kickHitbox.enabled = false;
    }
}
