using UnityEngine;

public class PlayerJumpState : IPlayerState
{
    private Rigidbody rigidbody;
    private float jumpForce = 5f;

    public void Enter(Player_Action player)
    {
        rigidbody = player.GetComponent<Rigidbody>();
        Debug.Log("상태 진입 : Jump");
        player.Animator.SetTrigger("IsJump");
    }

    public void Execute(Player_Action player)
    {
        rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        player.IsGrounded = false;
    }

    public void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈 : Jump");
    }
}
