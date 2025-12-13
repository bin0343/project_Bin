using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class EnemyAttackHItbox : MonoBehaviour
{
    [SerializeField]
    private Collider hitbox;

    private bool hasHit = false;
    Weapon_Brute weapon_Brute;

    private void Awake()
    {
        weapon_Brute = GetComponentInChildren<Weapon_Brute>();
        hitbox = GetComponentInChildren<Collider>();
        hitbox.enabled = false;
    }

    public void EnableHitbox()
    {
        hitbox.enabled = true;
        hasHit = false; 
    }

    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }

    public void ResetHasHit()
    {
        weapon_Brute.hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player") || other.CompareTag("Companion"))
        {
            Debug.Log("АјАн!");
            hasHit = true;
        }
    }
}
