using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    public static int comboStep = 0;

    protected override PlayerAnimState GetAnimState()
    {
        switch (comboStep)
        {
            case 1: return PlayerAnimState.Attack1;
            case 2: return PlayerAnimState.Attack2;
            case 3: return PlayerAnimState.Attack3;
            default: return PlayerAnimState.Idle;
        }
    }

    public override void Enter(Player_Action player)
    {
        comboStep++;
        if (comboStep > 3)
        {
            comboStep = 1;
        }

        player.canReceiveInput = false;   // 처음 들어올 때는 입력 잠금
        base.Enter(player);               // Animator의 ActionState 세팅
        Debug.Log($"상태 진입: Attack {comboStep}");
    }

    public override void Execute(Player_Action player)
    {
        player.Move.HandleRotation();
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);
        bool isAttackAnimation = stateInfo.IsTag("Attack");

        // 입력을 받을 수 있을 때만 다음 콤보 입력 허용
        if (player.canReceiveInput && Input.GetMouseButtonDown(0))
        {
            player.ChangeState(new PlayerAttackState());
            return;
        }

        // 공격 애니메이션이 끝났다면 Idle 상태로 복귀
        if (isAttackAnimation && stateInfo.normalizedTime >= 0.95f)
        {
            player.ChangeState(new PlayerIdleState());

            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                player.ChangeState(new PlayerMoveState());
                return;
            }
        }

        
    }

    public override void Exit(Player_Action player)
    {
        // 나갈 때 입력 다시 잠금
        player.canReceiveInput = false;
    }

    public static void ResetCombo()
    {
        comboStep = 0;
    }
}
