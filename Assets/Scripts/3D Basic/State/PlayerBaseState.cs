using UnityEngine;

public enum PlayerAnimState
{
    Idle,   //0
    Walk,   //1
    Run,    //2
    Attack1,    //3
    Attack2,    //4
    Attack3,    //5
    RunningAttack,  //6
    Jump,   //7
    RunningJump,   //8 
    Sit,    //9
    SitMove,    //10
    Kick,   //11
    Shield, //12
    Dead,    //13
    Hit,     //14
    AttackBlocked     //15
}

public abstract class PlayerBaseState : IPlayerState
{
    protected abstract PlayerAnimState GetAnimState();
    public virtual void Enter(Player_Action player)
    {
        player.animator.SetInteger("ActionState", (int)GetAnimState());
    }

    public abstract void Execute(Player_Action player);

    public abstract void Exit(Player_Action player);

    protected virtual void HandleCommonSkillInput(Player_Action player)     //스킬 입력 공통
    {
        for (int i = 0; i < player.playerSkills.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.F1 + i) && player.IsGrounded)
            {
                player.HandleSkillInput(i);
                return;
            }
        }
    }

    protected virtual void HandleCommonItemInput(Player_Action player)      //아이템 입력 공통
    {
        for (int i = 0; i < 4; i++) // 퀵슬롯은 4개로 가정
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Player_Action의 아이템 처리 함수 호출
                player.HandleItemInput(i);
                return;
            }
        }
    }

    protected virtual void HandleMovementInput(Player_Action player, float speedModifier = 1.0f)
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (moveInput.magnitude > 0)
        {
            float speed = player.move.GetAdjustedSpeed(moveInput) * speedModifier;
            player.move.HandleMovement(moveInput, speed);
            player.move.HandleRotation();

            player.animator.SetFloat("Horizontal", moveInput.x);
            player.animator.SetFloat("Vertical", moveInput.y);
        }

        // 3. 달리기/걷기 애니메이션 상태 전환
        PlayerAnimState expectedAnimState = Input.GetKey(KeyCode.LeftShift) ? PlayerAnimState.Run : PlayerAnimState.Walk;
        if (player.animator.GetInteger("ActionState") != (int)expectedAnimState)
        {
            // 현재 상태가 Move가 아닐 때도 애니메이션이 Run/Walk로 바뀌는 것을 방지하기 위해
            // 현재 상태가 Move 상태일 때만 애니메이션을 변경하도록 조건을 추가할 수 있습니다.
            if (player.currentState is PlayerMoveState)
            player.animator.SetInteger("ActionState", (int)expectedAnimState);
        }

    }
}
