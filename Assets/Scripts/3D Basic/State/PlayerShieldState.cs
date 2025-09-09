using UnityEngine;

public class PlayerShieldState : IPlayerState
{
    public void Enter(Player_Action player)
    {
        player.Animator.SetBool("IsShield", true);
    }

    public void Execute(Player_Action player)
    {

    }

    public void Exit(Player_Action player)
    {

    }
}
