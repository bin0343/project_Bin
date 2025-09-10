using UnityEngine;

public class PlayerDeadState : IPlayerState
{
    //private bool isDead;

    public void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Dead");
        player.Animator.SetTrigger("IsDie");
        player.IsDead = true;
    }

    public void Execute(Player_Action player)
    {
        
    }

    public void Exit(Player_Action player)
    {

    }
}
