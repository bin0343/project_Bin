using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AnimationEvent : MonoBehaviour
{
    private EnemyBase enemy;
    private Weapon_EnemyDefense defense;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyBase>();
        defense = GetComponentInParent<EnemyBase>().GetComponentInChildren<Weapon_EnemyDefense>(true);
    }

    public void StartAttackTrail()
    {
        if (enemy != null)
            enemy.slashTrail.emitting = true;
    }

    public void EndAttackTrail()
    {
        if (enemy != null)
            enemy.slashTrail.emitting = false;
    }

    public void StartAttack()
    {
        enemy.isAttacking = true;
    }

    public void EndAttack()
    {
        enemy.isAttacking = false;
    }

    public void EnableDefenseCollider()
    {
        if (defense != null)
        {
            defense.SetActiveDefense(true);
        }
    }

    public void DisableDefenseCollider()
    {
        if (defense != null)
        {
            defense.SetActiveDefense(false);
        }
    }
}
