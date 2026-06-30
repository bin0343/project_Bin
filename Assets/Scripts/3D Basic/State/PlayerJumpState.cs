using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private float jumpForce = 7.5f;
    private float jumpSpeed;
    private bool wasRunning;

    protected override PlayerAnimState GetAnimState()
    {
        return wasRunning ? PlayerAnimState.RunningJump : PlayerAnimState.Jump;
    }

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Jump");
        base.Enter(player);

        player.ForceJumpAirborne();

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        this.jumpSpeed = player.move.GetAdjustedSpeed(moveInput);

        player.rigidbody.velocity = new Vector3(player.rigidbody.velocity.x, 0, player.rigidbody.velocity.z);
        player.rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public override void Execute(Player_Action player)
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        player.move.HandleMovement(moveInput, jumpSpeed);
        player.move.HandleRotation();

        if (player.IsGrounded && player.rigidbody.velocity.y <= 0.1f)
        {
            // 움직임 입력이 있으면 Move, 없으면 Idle로 복귀
            if (moveInput.magnitude > 0.01f)
            {
                player.ChangeState(new PlayerMoveState());
            }
            else
            {
                player.ChangeState(new PlayerIdleState());
            }
            return;
        }

        base.HandleCommonItemInput(player);
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈 : Jump");
    }
}
