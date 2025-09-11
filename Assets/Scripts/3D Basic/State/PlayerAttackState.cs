using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    public override void Enter(Player_Action player)
    {
        Debug.Log("상태진입 : Attack");
        player.Animator.SetTrigger("IsAttacking");
        player.IsAttacking = true;
        player.canReceiveInput = false; // 첫 입력 잠금
    }

    public override void Execute(Player_Action player)
    {
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);

        if (player.canReceiveInput && Input.GetMouseButtonDown(0))
        {
            player.Animator.SetTrigger("IsAttacking");
            player.canReceiveInput = false; // 다시 입력 잠금
        }

        if (!player.IsAttacking && stateInfo.IsTag("Attack"))
        {
            player.ChangeState(new PlayerIdleState());
        }

        base.HandleCommonItemInput(player);
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈 : Attack");
        player.IsAttacking = false;
        player.Animator.ResetTrigger("IsAttacking");
        player.canReceiveInput = true; // 나가면서 초기화 (다음 공격 준비)
    }
}
