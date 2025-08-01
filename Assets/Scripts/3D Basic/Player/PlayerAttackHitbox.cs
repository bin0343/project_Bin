using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField]
    private Collider Hitbox;

    private bool HasHit = false;
    Weapon_Player Weapon_Player;

    private void Awake()
    {
        Weapon_Player = GetComponentInChildren<Weapon_Player>();
        Hitbox = GetComponentInChildren<Collider>();
        Hitbox.enabled = false;
    }

    public void EnableHitbox()
    {
        Hitbox.enabled = true;
        HasHit = false;
    }

    public void DisableHitbox()
    {
        Hitbox.enabled = false;
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
            Debug.Log("АјАн!");
            HasHit = true;
        }
    }
}
