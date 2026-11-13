using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AnimationEvent : MonoBehaviour
{
    private EnemyBase enemy;
    private EnemyAttackHItbox attackHItbox;
    private BossPatternExecutor bossPatternExecutor;
    private CharacterSoundPlayer soundPlayer;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyBase>();
        attackHItbox = GetComponent<EnemyAttackHItbox>();
        bossPatternExecutor = GetComponentInParent<BossPatternExecutor>();
        soundPlayer = GetComponentInParent<CharacterSoundPlayer>();
    }

    public void StartAttackTrail()
    {
        if (enemy != null && enemy.slashTrail != null)
            enemy.slashTrail.emitting = true;
    }

    public void EndAttackTrail()
    {
        if (enemy != null && enemy.slashTrail != null) 
            enemy.slashTrail.emitting = false;
    }

    public void StartAttack()
    {
        if (enemy != null) enemy.isAttacking = true;

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

    public void LeapImpactEvent()
    {
        bossPatternExecutor?.TriggerLeapImpact();
    }

    public void FootstepSound()
    {
        soundPlayer?.PlayFootstep();
    }

    public void AttackSound(int attackIndex)
    {
        soundPlayer?.PlayAttack(attackIndex);
    }

    public void JumpSound()
    {
        soundPlayer?.PlayJump();
    }
}
