using UnityEngine;

public class PlayerSitState : IPlayerState
{
    public void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : Sit");
        player.Animator.SetTrigger("SitTrigger");
        
    }

    public void Execute(Player_Action player)
    {
        player.Animator.SetBool("IsSitting", true);

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public void Exit(Player_Action player)
    {
        player.Animator.SetBool("IsSitting", false);
        player.Animator.ResetTrigger("SitTrigger");
    }
}
