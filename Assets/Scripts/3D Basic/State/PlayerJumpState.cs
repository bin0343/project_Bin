using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private float jumpForce = 5f;
    private float jumpSpeed;
    private bool wasRunning;

    protected override PlayerAnimState GetAnimState()
    {
        return wasRunning ? PlayerAnimState.RunningJump : PlayerAnimState.Jump;
    }

    public override void Enter(Player_Action player)
    {
        this.wasRunning = Input.GetKey(KeyCode.LeftShift);

        Debug.Log("상태 진입 : Jump");
        base.Enter(player);

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        this.jumpSpeed = player.Move.GetAdjustedSpeed(moveInput);

        player.Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        player.IsGrounded = false;
    }

    public override void Execute(Player_Action player)
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        player.Move.HandleMovement(moveInput, jumpSpeed);
        player.Move.HandleRotation();

        if (player.IsGrounded)
        {
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
