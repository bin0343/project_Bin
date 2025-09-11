using UnityEngine;

public class PlayerKickState : PlayerBaseState
{
    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : kick");
        player.Animator.SetTrigger("IsKick");
        //player.IsKick = true;
    }

    public override void Execute(Player_Action player)
    {
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Kick") && stateInfo.normalizedTime >= 0.9f)
        {
            player.ChangeState(new PlayerIdleState());
        }

        base.HandleCommonItemInput(player);
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈 : kick");
    }
}
