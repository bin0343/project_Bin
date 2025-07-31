using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class EnemyAttackHItbox : MonoBehaviour
{
    [SerializeField]
    private Collider Hitbox;

    private bool HasHit = false;

    private void Awake()
    {
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
