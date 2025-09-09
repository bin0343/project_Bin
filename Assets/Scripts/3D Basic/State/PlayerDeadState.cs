using UnityEngine;

public class PlayerDeadState : IPlayerState
{
    //private bool isDead;

    public void Enter(Player_Action player)
    {
        player.Animator.SetTrigger("IsDie");
    }

    public void Execute(Player_Action player)
    {
        player.IsDead = true;
    }

    public void Exit(Player_Action player)
    {

    }
}
