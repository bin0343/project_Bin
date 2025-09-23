using UnityEngine;

public class PlayerKickState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Kick;
    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : kick");
        player.animator.SetFloat("Horizontal", 0);
        player.animator.SetFloat("Vertical", 0);
        base.Enter(player);
        
    }

    public override void Execute(Player_Action player)
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

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
