using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AnimationEvents : MonoBehaviour
{
    Player_Action Action;
    Player_Move Move;
    public Animator animator;

    [Header("Attack Effect")]
    [SerializeField] private TrailRenderer slashTrail;

    void Start()
    {
        Action = GetComponentInParent<Player_Action>();
        slashTrail.emitting = false;
    }

    public void KickEnd()
    {
        Action.IsKick = false;
    }

    public void BuffStart()
    {
        Action.IsBuff = true;
    }

    public void BuffEnd()
    {
        Action.IsBuff = false;
    }

    public void OnJumpAttackEnd()
    {
        Action.IsGrounded = true;
    }

    public void AttackStart()
    {
        Action.IsAttacking = true;
    }

    public void AttackEnd()
    {
        Action.IsAttacking = false;
    }

    public void OnAttackCombo()
    {
        Action.canReceiveInput = true;
    }

    public void StartAttackTrail()
    {
        slashTrail.emitting = true;
    }

    public void EndAttackTrail()
    {
        slashTrail.emitting = false;
    }

    public void ResetRandomIdle()
    {
        Action.Animator.SetInteger("RandomIdleIndex", 0);
    }

    public void OnComboWindowOpen()
    {
        // 이제 Action의 canReceiveInput을 직접 true로 설정합니다.
        Action.canReceiveInput = true;
    }

    /// <summary>
    /// 공격 애니메이션이 완전히 끝났을 때 호출
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        // Action에게 "공격 애니메이션 끝났어!" 라는 신호를 보냅니다.
        Action.OnAnimationEvent(Player_Action.AnimationEventType.ATTACK_ANIMATION_END);
    }
}
