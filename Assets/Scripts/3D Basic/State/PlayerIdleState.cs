using UnityEngine;

public class PlayerIdleState : IPlayerState
{
    private float IdleTimer;
    private const float IdleDelay = 5f;
    private Player_Stat stat;

    public void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Idle");

        player.Animator.SetFloat("Horizontal", 0);
        player.Animator.SetFloat("Vertical", 0);
        IdleTimer = 0f;
    }

    public void Execute(Player_Action player)
    {
        //공격 상태 전환
        if (Input.GetMouseButtonDown(0) && player.IsGrounded)
        {
            player.ChangeState(new PlayerAttackState());
            return;
        }

        //방어 상태 전환
        if (Input.GetMouseButtonDown(1))
        {
            player.ChangeState(new PlayerShieldState());
            return;
        }

        //앉기 상태 전환
        if (Input.GetKey(KeyCode.LeftControl))
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

        if (stat.CurrentHP <= 0)
        {
            player.ChangeState(new PlayerDeadState());
            return;
        }

        for (int i = 0; i < player.playerSkills.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.F1 + i) && player.IsGrounded)
            {
                //player.HandleSkillInput(i);
                return;
            }
        }
    }

    public void Exit(Player_Action player)
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
