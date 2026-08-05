using UnityEngine;

public class PlayerStopState : PlayerBaseState
{
    private float stopTimer;
    private float stopDuration = 0.35f;

    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Idle;

    public override void Enter(Player_Action player)
    {
        stopTimer = 0f;

        player.rigidbody.velocity = new Vector3(0, player.rigidbody.velocity.y, 0);
        player.move.ForceMove(Vector3.zero, 0f);
        player.animator.SetInteger("ActionState", -1);
        player.animator.CrossFadeInFixedTime("Avoid_Stop", 0.1f);
    }

    public override void Execute(Player_Action player)
    {
        stopTimer += Time.deltaTime;

        player.move.ForceMove(Vector3.zero, 0f);

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && player.IsGrounded && PlayerRollState.CanDash)
        {
            if (Account_Manager.Instance.TryUseStamina(Account_Manager.Instance.rollStaminaCost))
            {
                player.ChangeState(new PlayerRollState());
                return;
            }
        }

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (moveInput.magnitude > 0.01f)
        {
            player.ChangeState(new PlayerMoveState());
            return;
        }

        if (stopTimer >= stopDuration)
        {
            player.rigidbody.velocity = new Vector3(0, player.rigidbody.velocity.y, 0);
            player.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(Player_Action player)
    {
    }
}
