using UnityEngine;

public class PlayerAttackState : IPlayerState
{
    private int comboStep;

    public void Enter(Player_Action player)
    {
        Debug.Log("상태진입 : Attack");
        comboStep = 1;
        player.Animator.SetTrigger("IsAttacking");
        player.IsAttacking = true;
        player.canReceiveInput = false; // 첫 입력 잠금
    }

    public void Execute(Player_Action player)
    {
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);

        // canReceiveInput은 애니메이션 이벤트에서 true로 바뀜
        if (player.canReceiveInput && Input.GetMouseButtonDown(0))
        {
            comboStep++;
            player.Animator.SetTrigger("IsAttacking");
            player.canReceiveInput = false; // 다시 입력 잠금
        }

        // 애니메이션이 끝나면 Idle로 전환
        if (stateInfo.normalizedTime >= 1.0f && stateInfo.IsTag("Attack"))
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈 : Attack");
        player.IsAttacking = false;
        player.Animator.ResetTrigger("IsAttacking");
        player.canReceiveInput = true; // 나가면서 초기화 (다음 공격 준비)
    }
}
