using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private float jumpForce = 5f;

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Jump");
        player.Animator.SetTrigger("IsJump");
        player.Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        player.IsGrounded = false;
    }

    public override void Execute(Player_Action player)
    {
        if (player.IsGrounded)
        {
            player.ChangeState(new PlayerIdleState());
        }

        base.HandleCommonItemInput(player);
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈 : Jump");
    }
}
