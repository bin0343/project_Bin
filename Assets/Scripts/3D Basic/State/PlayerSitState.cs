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
        base.HandleMovementInput(player, 0.5f);
        player.Animator.SetBool("IsSitting", true);

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            player.ChangeState(new PlayerIdleState());
        }

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            player.ChangeState(new PlayerSitMoveState());
            return;
        }
    }

    public override void Exit(Player_Action player)
    {
    }
}
