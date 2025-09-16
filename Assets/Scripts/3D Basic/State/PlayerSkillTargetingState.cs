using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillTargetingState : PlayerBaseState
{
    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Idle;
    public override void Enter(Player_Action player)
    {
        Debug.Log("상태 진입: Skill Targeting");

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

        player.targetingController.EnterTargetingMode(areaSkill, player.transform);
    }

    public override void Execute(Player_Action player)
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (moveInput.magnitude > 0.01f)
        {
            float speed = player.Move.GetAdjustedSpeed(moveInput);
            player.Move.HandleMovement(moveInput, speed);
            player.Move.HandleRotation();

            player.Animator.SetFloat("Horizontal", moveInput.x);
            player.Animator.SetFloat("Vertical", moveInput.y);

            PlayerAnimState expectedAnimState = Input.GetKey(KeyCode.LeftShift) ? PlayerAnimState.Run : PlayerAnimState.Walk;
            if (player.Animator.GetInteger("ActionState") != (int)expectedAnimState)
            {
                player.Animator.SetInteger("ActionState", (int)expectedAnimState);
            }
        }
        else
        {
            if (player.Animator.GetInteger("ActionState") != (int)PlayerAnimState.Idle)
            {
                player.Animator.SetInteger("ActionState", (int)PlayerAnimState.Idle);
            }
        }
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("상태 이탈: Skill Targeting");
    }
}
