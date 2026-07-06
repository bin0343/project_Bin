using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AnimationEvents : MonoBehaviour
{
    Player_Action action;
    public Animator animator;
    private Player_AttackVFX attackVFX;

    /*[Header("Attack Effect")]
    [SerializeField] public TrailRenderer slashTrail;*/

    void Start()
    {
        action = GetComponentInParent<Player_Action>();
        attackVFX = GetComponentInParent<Player_AttackVFX>();
        //slashTrail.emitting = false;
        action.currentWeapon?.StopTrail();
    }

    public void BuffStart()
    {
        if (action == null)
        {
            action = GetComponentInParent<Player_Action>();
        }

        action.IsBuff = true;
        action.ExecuteSkillEffectEvent();
    }

    public void BuffEnd()
    {
        action.IsBuff = false;
    }

    public void PlaySlashVFX()
    {
        attackVFX?.PlaySlashVFX();
    }

    public void AttackStart()
    {
        action.IsAttacking = true;
    }

    public void AttackEnd()
    {
        action.IsAttacking = false;
    }

    public void OnAttackCombo()
    {
        action.OnAnimationEvent(Player_Action.AnimationEventType.COMBO_WINDOW_OPEN);
    }

    public void StartAttackTrail()
    {
        action.currentWeapon?.StartTrail();
    }

    public void EndAttackTrail()
    {
        action.currentWeapon?.StopTrail();
    }

    public void EnableAttackHitbox()
    {
        action.currentWeapon?.EnableHitbox();
    }

    public void DisableAttackHitbox()
    {
        action.currentWeapon?.DisableHitbox();
    }

    public void ResetRandomIdle()
    {
        action.animator.SetInteger("RandomIdleIndex", 0);
    }

    public void OnComboWindowOpen()
    {
        action.OnAnimationEvent(Player_Action.AnimationEventType.COMBO_WINDOW_OPEN);
    }

    public void OnAttackAnimationEnd()
    {
        action.OnAnimationEvent(Player_Action.AnimationEventType.ATTACK_ANIMATION_END);
    }

    public void OnHitAnimationEnd()
    {
        (action.currentState as PlayerHitState)?.OnHitAnimationEnd(action);
    }

    public void SkillEffectEvent()
    {
        if (action == null)
        {
            action = GetComponentInParent<Player_Action>();
        }

        action.ExecuteSkillEffectEvent();
    }
}
