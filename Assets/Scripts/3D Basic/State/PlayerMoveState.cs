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
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        float speed = player.Move.GetAdjustedSpeed(moveInput);
        player.Move.HandleMovement(moveInput, speed);
        player.Move.HandleRotation();

        
        player.Animator.SetFloat("Horizontal", moveInput.x);
        player.Animator.SetFloat("Vertical", moveInput.y);

        if (Input.GetMouseButtonDown(0) && player.IsGrounded)
        {
            if (player.Animator.GetInteger("ActionState") == (int)PlayerAnimState.Run)
            {
                player.ChangeState(new PlayerRunningAttackState());
            }
            else
            {
                player.ChangeState(new PlayerAttackState());
            }
            return;
        }

        if (moveInput.magnitude == 0)
        {
            player.ChangeState(new PlayerIdleState());
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && player.IsGrounded)
        {
            player.ChangeState(new PlayerJumpState());
            return;
        }

        PlayerAnimState expectedAnimState = Input.GetKey(KeyCode.LeftShift) ? PlayerAnimState.Run : PlayerAnimState.Walk;
        if (player.Animator.GetInteger("ActionState") != (int)expectedAnimState)
        {
            player.Animator.SetInteger("ActionState", (int)expectedAnimState);
        }

        // 5. 공통 입력(스킬, 아이템) 확인
        base.HandleCommonSkillInput(player);
        base.HandleCommonItemInput(player);
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈: Move");
        player.Animator.SetFloat("Horizontal", 0);
        player.Animator.SetFloat("Vertical", 0);
    }
}
