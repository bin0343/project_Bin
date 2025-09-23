using UnityEngine;

public class PlayerShieldState : PlayerBaseState
{
    private Shield_Player shield;
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Shield;
    public override void Enter(Player_Action player)
    {
        base.Enter(player);
        Debug.Log("상태진입 : Shield");
        //player.Animator.SetBool("IsShield", true);

        shield = player.GetComponentInChildren<Shield_Player>();
        if (shield != null)
        {
            // 방패 콜라이더 활성화
            shield.SetActiveShield(true);
        }
    }

    public override void Execute(Player_Action player)
    {
        base.HandleMovementInput(player);
        if(Input.GetMouseButtonUp(1))
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(Player_Action player)
    {
        //player.animator.SetBool("IsShield", false);
        Debug.Log("상태 이탈 : Shield");
    }
}
