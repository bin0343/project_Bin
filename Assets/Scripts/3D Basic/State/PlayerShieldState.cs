using UnityEngine;

public class PlayerShieldState : IPlayerState
{
    public void Enter(Player_Action player)
    {
        Debug.Log("상태진입 : Shield");
        player.Animator.SetBool("IsShield", true);
    }

    public void Execute(Player_Action player)
    {
        if(Input.GetMouseButtonUp(1))
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public void Exit(Player_Action player)
    {
        player.Animator.SetBool("IsShield", false);
        Debug.Log("상태 이탈 : Shield");
    }
}
