using UnityEngine;

public class PlayerJumpState : IPlayerState
{
    private float jumpForce = 5f;

    public void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Jump");
        player.Animator.SetTrigger("IsJump");
        player.Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        player.IsGrounded = false;
    }

    public void Execute(Player_Action player)
    {
        if (player.IsGrounded)
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈 : Jump");
    }
}
