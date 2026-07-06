using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AnimationEvent : MonoBehaviour
{
    private EnemyBase enemy;
    private EnemyAttackHItbox attackHItbox;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyBase>();
        attackHItbox = GetComponent<EnemyAttackHItbox>();
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
        if (enemy != null)
            enemy.isAttacking = true;

        if (attackHItbox != null)
            attackHItbox.EnableHitbox();
    }

    public void EndAttack()
    {
        if (enemy != null)
            enemy.isAttacking = false;

        if (attackHItbox != null)
            attackHItbox.DisableHitbox();
    }

    public void ForceEndAttack()
    {
        EndAttack();
        EndAttackTrail();
    }
}
