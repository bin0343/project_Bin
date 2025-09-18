using UnityEngine;

public class PlayerRunningAttackState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.RunningAttack;

    public override void Enter(Player_Action player)
    {
        base.Enter(player);
        Debug.Log("상태 진입: Running Slash");

        player.UseRunningAttack();

        Vector3 moveDir = player.Move.CharacterBody.forward;
        player.Rigidbody.velocity = moveDir * 8f;
    }

    public override void Execute(Player_Action player)
    {
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 0.95f)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                player.ChangeState(new PlayerMoveState());
            }
            else
            {
                player.ChangeState(new PlayerIdleState());
            }
        }
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈: Running Slash");
        player.Rigidbody.velocity = Vector3.zero;
    }
}
