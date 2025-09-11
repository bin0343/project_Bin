using UnityEngine;

public class PlayerRunningAttackState : PlayerBaseState
{
    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입: Running Slash");

        player.Animator.SetTrigger("RunningSlash");     //트리거라서 수정을 할 때 이렇게 하면 일일이 다 건드려서 수정해야함(최상위 클래스에서 한 번 수정하면 모든게 수정되게 바꿀것)

        Vector3 moveDir = player.Move.CharacterBody.forward;
        player.Rigidbody.velocity = moveDir * player.Move.CharacterRunSpeed;
    }

    public override void Execute(Player_Action player)
    {
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 0.95f)
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈: Running Slash");
        player.Rigidbody.velocity = Vector3.zero;
    }
}
