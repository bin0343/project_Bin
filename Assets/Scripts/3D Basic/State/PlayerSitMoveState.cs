using UnityEngine;

public class PlayerSitMoveState : PlayerBaseState
{
    private float noInputTimer;
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.SitMove;
    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입 : SitMove");
        base.Enter(player);
        noInputTimer = 0f;
    }

    public override void Execute(Player_Action player)
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (moveInput.magnitude > 0.01f)
        {
            base.HandleMovementInput(player, 0.5f);
            noInputTimer = 0f;
        }
        else
        {
            noInputTimer += Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            player.ChangeState(new PlayerIdleState());
            return;
        }

        if (noInputTimer > 0.01f)
        {
            player.ChangeState(new PlayerSitState());
            return;
        }
    }

    public override void Exit(Player_Action player)
    {
    }
}
