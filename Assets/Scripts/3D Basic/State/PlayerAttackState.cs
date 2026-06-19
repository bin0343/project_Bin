using UnityEngine;

public class PlayerAttackState : PlayerBaseState, IStateAnimationEvents
{
    public static int comboStep = 0;

    //상태 진입 직후 애니메이션 전환이 완료되었는지 확인하는 플래그
    private bool isTransitionFinished = false;

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
        if (Player_Equipment.instance != null)
        {
            Player_Equipment.instance.EnterCombatState();
        }

        player.CanRotate = false;
        if (Cursor.visible || Cursor.lockState == CursorLockMode.None)
        {
            player.move.LookAtMouse();
        }
        else
        {
            player.move.AlignToCameraForward();
        }

        isTransitionFinished = false;

        comboStep++;
        if (comboStep > 3)
        {
            comboStep = 1;
        }
        player.SetComboStep(comboStep);
        player.canReceiveInput = false;
        base.Enter(player);
    }

    public override void Execute(Player_Action player)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && player.IsGrounded && !player.IsPointerOverUI())
        {
            if (Account_Manager.instance.TryUseStamina(Account_Manager.instance.rollStaminaCost))
            {
                player.ChangeState(new PlayerRollState());
                return; // 상태 전환 후 아래 로직 실행 안 함
            }
            else
            {
                Debug.Log("스태미나가 부족해서 구를 수 없습니다!");
            }
        }

        if (!isTransitionFinished)
        {
            AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
            int expectedAnimHash = Animator.StringToHash(GetAnimState().ToString());

            if (stateInfo.shortNameHash == expectedAnimHash)
            {
                isTransitionFinished = true;
            }
        }

        if (player.canReceiveInput && Input.GetMouseButtonDown(0))
        {
            if (comboStep < 3)
            {
                player.ChangeState(new PlayerAttackState());
                return;
            }
        }
    }

    public override void Exit(Player_Action player)
    {
        player.CanRotate = true;
        player.animEvents?.EndAttackTrail();
        player.canReceiveInput = false;
        player.IsAttacking = false;
    }

    public static void ResetCombo()
    {
        comboStep = 0;
    }

    public void OnAnimationEvent(Player_Action.AnimationEventType eventType, Player_Action player)
    {
        if (!isTransitionFinished)
        {
            return;
        }

        if (eventType == Player_Action.AnimationEventType.ATTACK_ANIMATION_END)
        {
            ResetCombo();

            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                player.ChangeState(new PlayerMoveState());
            }
            else
            {
                player.ChangeState(new PlayerIdleState());
            }
        }
    }
}