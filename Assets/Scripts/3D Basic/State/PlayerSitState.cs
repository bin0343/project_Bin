using UnityEngine;

public class PlayerSitState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Sit;
    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Sit");
        base.Enter(player);
    }

    public override void Execute(Player_Action player)
    {
        player.Animator.SetBool("IsSitting", true);

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(Player_Action player)
    {
        player.Animator.SetBool("IsSitting", false);
    }
}
