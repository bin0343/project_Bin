using UnityEngine;

public class PlayerWalkingState : IPlayerState_Move
{
    public void Enter(Player_Move player)
    {
        Debug.Log("이동 상태 진입 : Walking");
        player.Animator.SetBool("IsMoving", true);
    }

    public void Execute(Player_Move player)
    {
        //player.HandleMovement();
        player.HandleRotation();
        //player.HandleRun();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //player.ChangeMoveState(new PlayerRunningState());
            return;
        }

        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            //player.ChangeMoveState(new PlayerStandingState());
            return;
        }
    }

    public void Exit(Player_Move player)
    {

    }
}
