using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState()
    {
        return Input.GetKey(KeyCode.LeftShift) ? PlayerAnimState.Run : PlayerAnimState.Walk;
    }

    public override void Enter(Player_Action player)
    {
        Debug.Log("이동 상태 진입 : Move");
        base.Enter(player);
    }

    public override void Execute(Player_Action player)
    {
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            player.ChangeState(new PlayerIdleState());
            return;
        }

        if (Input.GetMouseButtonDown(0) && player.IsGrounded)
        {
            if (player.animator.GetInteger("ActionState") == (int)PlayerAnimState.Run)
            {
                if (player.CanUseRunningAttack())
                {
                    player.ChangeState(new PlayerRunningAttackState());
                }
                else
                {
                    Debug.Log("러닝 어택 쿨타임입니다!");
                }
            }
            else
            {
                player.ChangeState(new PlayerAttackState());
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            player.ChangeState(new PlayerKickState());
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && player.IsGrounded)
        {
            player.ChangeState(new PlayerJumpState());
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            player.ChangeState(new PlayerSitState());
            return;
        }

        base.HandleMovementInput(player);

        PlayerAnimState expectedAnimState = Input.GetKey(KeyCode.LeftShift) ? PlayerAnimState.Run : PlayerAnimState.Walk;
        if (player.animator.GetInteger("ActionState") != (int)expectedAnimState)
        {
            player.animator.SetInteger("ActionState", (int)expectedAnimState);
        }

        // 5. 공통 입력(스킬, 아이템) 확인
        base.HandleCommonSkillInput(player);
        base.HandleCommonItemInput(player);
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈: Move");
        player.animator.SetFloat("Horizontal", 0);
        player.animator.SetFloat("Vertical", 0);
    }
}
