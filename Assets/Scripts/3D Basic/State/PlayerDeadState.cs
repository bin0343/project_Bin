using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{
    //private bool isDead;

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Dead");
        player.Animator.SetTrigger("IsDie");
        player.IsDead = true;
    }

    public override void Execute(Player_Action player)
    {
        
    }

    public override void Exit(Player_Action player)
    {

    }
}
