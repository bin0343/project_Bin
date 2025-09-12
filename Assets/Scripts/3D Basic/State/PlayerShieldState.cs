using UnityEngine;

public class PlayerShieldState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Shield;
    public override void Enter(Player_Action player)
    {
        base.Enter(player);
        Debug.Log("상태진입 : Shield");
        //player.Animator.SetBool("IsShield", true);
    }

    public override void Execute(Player_Action player)
    {
        if(Input.GetMouseButtonUp(1))
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(Player_Action player)
    {
        player.Animator.SetBool("IsShield", false);
        Debug.Log("상태 이탈 : Shield");
    }
}
