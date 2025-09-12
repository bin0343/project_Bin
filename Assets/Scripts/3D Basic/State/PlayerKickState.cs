using UnityEngine;

public class PlayerKickState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Kick;
    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : kick");
        base.Enter(player);
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
