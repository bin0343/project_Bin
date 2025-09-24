using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackBlockState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.AttackBlocked;

    public override void Enter(Player_Action player)
    {
        base.Enter(player);

        player.IsAttacking = false;

        player.attackHitbox.DisableAttackHitbox();
    }

    public override void Execute(Player_Action player)
    {
        
    }

    public override void Exit(Player_Action player)
    {
        
    }

    public void OnBlockedAnimationEnd(Player_Action player)
    {
        player.ChangeState(new PlayerIdleState());
    }
}
