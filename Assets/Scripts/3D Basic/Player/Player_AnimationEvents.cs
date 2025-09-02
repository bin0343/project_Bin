using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AnimationEvents : MonoBehaviour
{
    Player_Action Action;
    Player_Move Move;
    public Animator animator;

    void Start()
    {
        Action = GetComponentInParent<Player_Action>();
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

    public void AttackEnd()
    {
        Action.IsAttacking = false;
    }

    public void AttackReset()
    {
        animator.ResetTrigger("IsAttacking");
    }

    public void OnAttackCombo()
    {
        Action.canReceiveInput = true;
    }
}
