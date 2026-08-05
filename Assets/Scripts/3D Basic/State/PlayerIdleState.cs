using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    private float idleTimer;
    private const float idleDelay = 5f;

    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Idle;

    public override void Enter(Player_Action player)
    {
        base.Enter(player);

        PlayerAttackState.ResetCombo();

        /*player.animator.SetFloat("Horizontal", 0);
        player.animator.SetFloat("Vertical", 0);*/
        idleTimer = 0f;
    }

    public override void Execute(Player_Action player)
    {
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && player.IsGrounded && !player.IsPointerOverUI())
        {
            if (Account_Manager.Instance.TryUseStamina(Account_Manager.Instance.rollStaminaCost))
            {
                player.ChangeState(new PlayerRollState());
            }
            
            return;
        }

        if (Input.GetButtonDown("Jump") && player.IsGrounded && !player.IsPointerOverUI())
        {
            player.ChangeState(new PlayerJumpState());
            return;
        }

        //공격 상태 전환
        if (Input.GetMouseButtonDown(0) && player.IsGrounded && !player.IsPointerOverUI())
        {
            player.ChangeState(new PlayerAttackState());
            return;
        }

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            player.ChangeState(new PlayerMoveState());
            return;
        }

        base.HandleCommonItemInput(player);
        base.HandleCommonSkillInput(player);

        HandleRandomIdle(player);
    }

    public override void Exit(Player_Action player)
    {
        player.animator.SetInteger("RandomIdleIndex", 0);
    }

    public void HandleRandomIdle(Player_Action player)
    {
        if (player.animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDelay)
            {
                int rand = Random.Range(1, 4);
                player.animator.SetInteger("RandomIdleIndex", rand);
                idleTimer = 0;
            }
        }
        else
        {
            idleTimer = 0;
        }
    }
}
