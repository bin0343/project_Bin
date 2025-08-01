using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class EnemyAttackHItbox : MonoBehaviour
{
    [SerializeField]
    private Collider Hitbox;

    private bool HasHit = false;
    Weapon_Brute Weapon_Brute;

    private void Awake()
    {
        Weapon_Brute = GetComponentInChildren<Weapon_Brute>();
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
        Weapon_Brute.HasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HasHit) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("АјАн!");
            HasHit = true;
        }
    }
}
