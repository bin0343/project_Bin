using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    private readonly bool useLaunchKnockDownAnimation;

    public PlayerHitState(bool useLaunchKnockDownAnimation = false)
    {
        this.useLaunchKnockDownAnimation = useLaunchKnockDownAnimation;
    }

    protected override PlayerAnimState GetAnimState()
    {
        if (useLaunchKnockDownAnimation)
        {
            return PlayerAnimState.LaunchKnockDown;
        }

        return PlayerAnimState.Hit;
    }

    public override void Enter(Player_Action player)
    {
        base.Enter(player);
    }

    public override void Execute(Player_Action player)
    {
    }

    public override void Exit(Player_Action player)
    {
    }

    public void OnHitAnimationEnd(Player_Action player)
    {
        player.NotifyHitAnimationEnded();
    }
}
