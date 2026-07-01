using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AnimationEvents : MonoBehaviour
{
    Player_Action action;
    public Animator animator;

    /*[Header("Attack Effect")]
    [SerializeField] public TrailRenderer slashTrail;*/

    void Start()
    {
        action = GetComponentInParent<Player_Action>();
        //slashTrail.emitting = false;
        action.currentWeapon?.StopTrail();
    }

    public void KickEnd()
    {
        action.IsKick = false;
    }

    public void BuffStart()
    {
        action.IsBuff = true;
    }

    public void BuffEnd()
    {
        action.IsBuff = false;
    }

    public void OnJumpAttackEnd()
    {
        action.IsGrounded = true;
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

    public void CameraShakeEvent()
    {
        if (Shared.MainCamera != null)
        {
            Shared.MainCamera.Shake(0.2f, 3f, 4);
        }
    }

    public void CameraShake_Attack()
    {
        if (Shared.MainCamera != null)
        {
            Shared.MainCamera.Shake(0.1f, 2f, 3);
        }
    }
}
