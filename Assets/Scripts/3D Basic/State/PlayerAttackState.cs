using UnityEngine;

public class PlayerAttackState : IPlayerState
{
    private bool comboAvailable;
    private int comboStep;

    public void Enter(Player_Action player)
    {
        Debug.Log("상태진입 : Attack");
        player.Animator.SetTrigger("IsAttacking");
        comboStep = 1;
    }

    public void Execute(Player_Action player)
    {
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 0.8f)
            comboAvailable = true;

        if (comboAvailable && Input.GetMouseButtonDown(0))
        {
            player.Animator.SetTrigger("IsAttacking");
            comboStep++;
            comboAvailable = false;
        }

        if (stateInfo.normalizedTime >= 1.0f && stateInfo.IsTag("Attack"))
        {
            //player.changeState(new PlayerIdleState());
        }
    }

    public void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈 : Attack");
    }
}
