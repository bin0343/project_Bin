using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    private float IdleTimer;
    private const float IdleDelay = 5f;

    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Idle;

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Idle");
        base.Enter(player);

        PlayerAttackState.ResetCombo();

        player.Animator.SetFloat("Horizontal", 0);
        player.Animator.SetFloat("Vertical", 0);
        IdleTimer = 0f;
    }

    public override void Execute(Player_Action player)
    {
        //공격 상태 전환
        if (Input.GetMouseButtonDown(0) && player.IsGrounded)
        {
            // Player_Move의 현재 상태를 확인합니다.
            if (player.Move.CurrentMoveState is PlayerRunningState)
            {
                // 만약 '달리는 중'이라면 RunningAttack 상태로 전환
                player.ChangeState(new PlayerRunningAttackState());
            }
            else
            {
                // 그 외의 경우(서있거나 걷는 중)에는 일반 Attack 상태로 전환
                player.ChangeState(new PlayerAttackState());
            }
            return;
        }

        //방어 상태 전환
        if (Input.GetMouseButtonDown(1))
        {
            player.ChangeState(new PlayerShieldState());
            return;
        }

        //앉기 상태 전환
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            player.ChangeState(new PlayerSitState());
            return;
        }

        //점프 상태 전환
        if (Input.GetKeyDown(KeyCode.Space) && player.IsGrounded)
        {
            player.ChangeState(new PlayerJumpState());
            return;
        }
        
        //킥 상태 전환
        if (Input.GetKeyDown(KeyCode.F))
        {
            player.ChangeState(new PlayerKickState());
            return;
        }

        if (player.Stat.CurrentHP <= 0)
        {
            player.ChangeState(new PlayerDeadState());
            return;
        }

        base.HandleCommonItemInput(player);
        base.HandleCommonSkillInput(player);

        HandleRandomIdle(player);
    }

    public override void Exit(Player_Action player)
    {
        player.Animator.SetInteger("RandomIdleIndex", 0);
        Debug.Log("상태 이탈 : Idle");
    }

    public void HandleRandomIdle(Player_Action player)
    {
        if (player.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
        {
            IdleTimer += Time.deltaTime;
            if (IdleTimer >= IdleDelay)
            {
                int rand = Random.Range(1, 4);
                player.Animator.SetInteger("RandomIdleIndex", rand);
                IdleTimer = 0;
            }
        }
        else
        {
            IdleTimer = 0;
        }
    }
}
