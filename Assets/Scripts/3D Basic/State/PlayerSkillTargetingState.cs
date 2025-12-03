using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillTargetingState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Idle;

    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입: Skill Targeting");

        // [수정] 스킬 조준 시 마우스 커서를 강제로 보이게 하고 잠금 해제
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (player.skillBeingAimed == null)
        {
            Debug.LogError("SkillTargetingState에 진입했지만 조준할 스킬이 없습니다! Idle 상태로 돌아갑니다.");
            player.ChangeState(new PlayerIdleState());
            return;
        }

        var areaSkill = player.skillBeingAimed.SkillData as Skill_AreaAttack;
        if (areaSkill == null)
        {
            Debug.LogError("조준하려는 스킬이 Skill_AreaAttack 타입이 아닙니다! Idle 상태로 돌아갑니다.");
            player.ChangeState(new PlayerIdleState());
            return;
        }

        // 컨트롤러 시작
        player.targetingController.EnterTargetingMode(areaSkill, player.transform);
    }

    public override void Execute(Player_Action player)
    {
        // TargetingController에서 이미 Update를 돌며 인디케이터를 움직이고 있으므로
        // 여기서는 플레이어 이동만 처리하면 됩니다.

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (moveInput.magnitude > 0.01f)
        {
            float speed = player.move.GetAdjustedSpeed(moveInput);
            player.move.HandleMovement(moveInput, speed);
            player.move.HandleRotation();

            if (player.animator.GetInteger("ActionState") != (int)PlayerAnimState.Run)
            {
                player.animator.SetInteger("ActionState", (int)PlayerAnimState.Run);
            }
        }
        else
        {
            if (player.animator.GetInteger("ActionState") != (int)PlayerAnimState.Idle)
            {
                player.animator.SetInteger("ActionState", (int)PlayerAnimState.Idle);
            }
        }
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈: Skill Targeting");
        // 상태를 빠져나갈 때 커서 설정을 원래대로 돌리고 싶다면 UI_Manager를 통해 처리하거나 여기서 처리
        // 예: UI_Manager.Instance.UpdateCursorState(); 
    }
}