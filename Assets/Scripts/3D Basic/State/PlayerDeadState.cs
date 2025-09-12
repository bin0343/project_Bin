using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Dead;

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Dead");
        base.Enter(player);
        player.IsDead = true;
    }

    public override void Execute(Player_Action player)
    {
        
    }

    public override void Exit(Player_Action player)
    {

    }
}
