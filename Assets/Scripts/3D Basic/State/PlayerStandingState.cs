using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStandingState : IPlayerState_Move
{
    public void Enter(Player_Move player)
    {
        player.Animator.SetBool("IsMoving", false);
        player.Animator.SetBool("IsRunning", false);
    }

    public void Execute(Player_Move player)
    {
        player.HandleRotation();

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            player.ChangeMoveState(new PlayerWalkingState());
        }
    }

    public void Exit(Player_Move player)
    {

    }
}
