using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState()
    {
        return PlayerAnimState.Hit;
    }

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입: Hit");
        base.Enter(player);
    }

    public override void Execute(Player_Action player)
    {
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈: Hit");
    }

    public void OnHitAnimationEnd(Player_Action player)
    {
        player.ChangeState(new PlayerIdleState());
    }
}
