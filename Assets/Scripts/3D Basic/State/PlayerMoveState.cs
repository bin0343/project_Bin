using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    private float exitTimer;
    private const float changeTimer = 0.1f;     //0.1초간 입력이 없어야
    protected override PlayerAnimState GetAnimState()
    {
        return PlayerAnimState.Run;
    }

    public override void Enter(Player_Action player)
    {
        base.Enter(player);
        exitTimer = 0f;
    }

    public override void Execute(Player_Action player)
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMoving = moveInput.magnitude > 0;
        bool jumpInput = Input.GetButtonDown("Jump");
        bool rollInput = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1);
        bool attackInput = Input.GetMouseButtonDown(0) && !player.IsPointerOverUI();

        if (!isMoving)
        {
            exitTimer += Time.deltaTime;

            if (exitTimer >= changeTimer)
            {
                player.ChangeState(new PlayerStopState());
                return;
            }
            
        }
        else
        {
            exitTimer = 0f;

            if (rollInput && player.IsGrounded && PlayerRollState.CanDash)
            {
                if (Account_Manager.Instance.TryUseStamina(Account_Manager.Instance.rollStaminaCost))
                {
                    player.ChangeState(new PlayerRollState());
                }
                return;
            }

            if (jumpInput && player.IsGrounded)
            {
                player.ChangeState(new PlayerJumpState());
                return;
            }

            if (attackInput)
            {
                player.ChangeState(new PlayerAttackState());
            }
            else
            {
                HandleMovementInput(player);
            }
        }

        HandleCommonSkillInput(player);
        HandleCommonItemInput(player);
    }

    public override void Exit(Player_Action player)
    {

    }
}
