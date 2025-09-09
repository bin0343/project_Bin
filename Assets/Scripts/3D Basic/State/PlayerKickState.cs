using UnityEngine;

public class PlayerKickState : IPlayerState
{
    //private bool IsKick = false;

    public void Enter(Player_Action player)
    {
        player.Animator.SetTrigger("IsKick");
    }

    public void Execute(Player_Action player)
    {
        player.IsKick = true;
    }

    public void Exit(Player_Action player)
    {

    }
}
