using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AnimationEvent : MonoBehaviour
{
    private EnemyBase enemy;

    void Start()
    {
        enemy = GetComponentInParent<EnemyBase>();
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
}
