using UnityEngine;

public class PlayerRunningState : IPlayerState_Move
{
    public void Enter(Player_Move player)
    {
        Debug.Log("이동 상태 진입 : Running");
        player.Animator.SetBool("IsRunning", true);
    }

    public void Execute(Player_Move player) 
    {
        if (Input.GetMouseButtonDown(0))
        {
            player.Action.ChangeState(new PlayerRunningAttackState());
            return; 
        }

        player.HandleRotation();
        player.HandleMovement();
        //player.HandleRun();

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            player.ChangeMoveState(new PlayerWalkingState());
            return;
        }

        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            player.ChangeMoveState(new PlayerStandingState());
            return;
        }
    }

    public void Exit(Player_Move player)
    {
        player.Animator.SetBool("IsRunning", false);
    }
}
