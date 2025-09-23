using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState()
    {
        // "Hit" 또는 "Damage" 등의 애니메이션 상태를 Enum에 추가해야 합니다.
        return PlayerAnimState.Hit;
    }

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입: Hit");
        base.Enter(player);
        // 여기서 SetInteger를 사용합니다. Trigger보다 상태를 명확히 제어할 수 있습니다.
    }

    public override void Execute(Player_Action player)
    {
        // 이 상태에서는 아무것도 하지 않습니다.
        // 애니메이션이 끝나면 자동으로 Idle로 돌아갑니다.
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈: Hit");
    }

    // 추가: 애니메이션 이벤트로부터 호출될 함수
    public void OnHitAnimationEnd(Player_Action player)
    {
        player.ChangeState(new PlayerIdleState());
    }
}
