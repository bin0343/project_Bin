using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState()
    {
        // MoveState는 Walk 또는 Run 애니메이션을 유동적으로 사용하므로
        // Enter 시점에 고정된 AnimState를 설정하지 않도록 Idle을 반환하거나,
        // 혹은 Enter에서 직접 초기 애니메이션(Walk)을 설정할 수 있습니다.
        return PlayerAnimState.Walk;
    }

    public override void Enter(Player_Action player)
    {
        base.Enter(player);
    }

    public override void Execute(Player_Action player)
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMoving = moveInput.magnitude > 0;
        bool jumpInput = Input.GetButtonDown("Jump");
        bool attackInput = Input.GetMouseButtonDown(0);

        // --- 2. 상태 전환 우선순위 결정 ---

        // 최우선 순위: 이동을 멈췄는가?
        if (!isMoving)
        {
            player.ChangeState(new PlayerIdleState());
        }
        // 이동 중일 때만 다른 입력들을 확인
        else
        {
            // 1순위: 점프
            if (jumpInput && player.IsGrounded)
            {
                player.ChangeState(new PlayerJumpState());
            }
            // 2순위: 공격 입력이 있었는가?
            else if (attackInput)
            {
                // 공격 입력이 있다면, 달리기 공격 시도인지 확인
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    // 달리기 공격 시도라면, 쿨타임을 확인
                    if (player.CanUseRunningAttack())
                    {
                        // 쿨타임이 정상이면 달리기 공격 상태로 전환
                        player.ChangeState(new PlayerRunningAttackState());
                    }
                    else
                    {
                        // 쿨타임이라면, 디버그 로그를 찍고 아무것도 하지 않음
                        // (상태를 바꾸지 않으므로 계속 달리기 상태를 유지)
                        Debug.Log("달리기 공격 쿨타임입니다!");
                    }
                }
                else // 달리기 공격 시도가 아니라면, 일반 공격 상태로 전환
                {
                    player.ChangeState(new PlayerAttackState());
                }
            }
            // 그 외 모든 경우 (점프도, 공격도 아닐 때): 계속 이동
            else
            {
                HandleMovementInput(player);
            }
        }

        // --- 3. 상태 전환과 별개로 매 프레임 확인해야 하는 입력들 ---
        HandleCommonSkillInput(player);
        HandleCommonItemInput(player);
    }

    public override void Exit(Player_Action player)
    {
        // Move 상태를 벗어날 때는 IsMoving 애니메이션 파라미터를 false로 설정하여
        // 다른 상태(예: 공격)에서 불필요한 움직임 애니메이션이 재생되는 것을 방지합니다.
        player.animator.SetBool("IsMoving", false);
    }
}
