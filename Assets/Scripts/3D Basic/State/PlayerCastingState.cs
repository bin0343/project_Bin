using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCastingState : PlayerBaseState
{
    private float castTime;
    private SkillHolder skillToUse;

    protected override PlayerAnimState GetAnimState() => PlayerAnimState.Idle;

    public PlayerCastingState(SkillHolder skill)
    {
        this.castTime = skill.SkillData.castTime;
        this.skillToUse = skill;
    }

    public override void Enter(Player_Action player)
    {
        if (skillToUse != null)
        {
            player.activeCastingSkill = skillToUse;

            player.animator.SetTrigger(skillToUse.SkillData.animTriggerName);
        }
    }

    public override void Execute(Player_Action player)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && player.IsGrounded && !player.IsPointerOverUI())
        {
            if (Account_Manager.instance.TryUseStamina(Account_Manager.instance.rollStaminaCost))
            {
                player.ChangeState(new PlayerRollState());
                return;
            }
        }

        castTime -= Time.deltaTime;

        if (castTime <= 0f)
        {
            player.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(Player_Action player)
    {
        Debug.Log("»óÅÂ ÀÌÅ»: Casting");
    }
}
