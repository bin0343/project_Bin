using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AnimationEvents : MonoBehaviour
{
    Player_Action action;
    public Animator animator;
    private Player_AttackVFX attackVFX;
    private CharacterSoundPlayer soundPlayer;

    void Start()
    {
        action = GetComponentInParent<Player_Action>();
        attackVFX = GetComponentInParent<Player_AttackVFX>();
        action.currentWeapon?.StopTrail();
        soundPlayer = GetComponentInParent<CharacterSoundPlayer>();
    }

    public void BuffStart()
    {
        if (action == null)
        {
            action = GetComponentInParent<Player_Action>();
        }

        action.IsBuff = true;
        action.ExecuteSkillEffectEvent();
    }

    public void BuffEnd()
    {
        action.IsBuff = false;
    }

    public void PlaySlashVFX()
    {
        attackVFX?.PlaySlashVFX();
    }

    public void AttackStart()
    {
        action.IsAttacking = true;
    }

    public void AttackEnd()
    {
        action.IsAttacking = false;
    }

    public void OnAttackCombo()
    {
        action.OnAnimationEvent(Player_Action.AnimationEventType.COMBO_WINDOW_OPEN);
    }

    public void StartAttackTrail()
    {
        action.currentWeapon?.StartTrail();
    }

    public void EndAttackTrail()
    {
        action.currentWeapon?.StopTrail();
    }

    public void EnableAttackHitbox()
    {
        action.currentWeapon?.ExecuteAttackFrame();
    }

    public void DisableAttackHitbox()
    {
        action.currentWeapon?.EndAttackFrame();
    }

    public void ResetRandomIdle()
    {
        action.animator.SetInteger("RandomIdleIndex", 0);
    }

    public void OnComboWindowOpen()
    {
        action.OnAnimationEvent(Player_Action.AnimationEventType.COMBO_WINDOW_OPEN);
    }

    public void OnAttackAnimationEnd()
    {
        action.OnAnimationEvent(Player_Action.AnimationEventType.ATTACK_ANIMATION_END);
    }

    public void OnHitAnimationEnd()
    {
        (action.currentState as PlayerHitState)?.OnHitAnimationEnd(action);
    }

    public void OnDeadAnimationEnd()
    {
        if (action == null)
        {
            action = GetComponentInParent<Player_Action>();
        }

        (action.currentState as PlayerDeadState)?.OnDeadAnimationEnd(action);
    }

    public void SkillEffectEvent()
    {
        if (action == null)
        {
            action = GetComponentInParent<Player_Action>();
        }

        if (action == null) return;

        // 검기 스킬의 발사 순간 차지 이펙트 종료
        if (action.activeCastingSkill != null && action.activeCastingSkill.SkillData is Skill_SwordWave)
        {
            StopSwordWaveCharge();
        }

        // 기존 스킬 효과 실행
        action.ExecuteSkillEffectEvent();
    }

    public void SwordWaveChargeStart()
    {
        if (action == null) action = GetComponentInParent<Player_Action>();

        if (action == null || action.activeCastingSkill == null) return;

        if (!(action.activeCastingSkill.SkillData is Skill_SwordWave)) return;

        // 현재 장착 중인 무기에서 차지 이펙트 실행
        action.currentWeapon?.StartSwordWaveCharge();
    }

    public void StopSwordWaveCharge()
    {
        if (action == null) action = GetComponentInParent<Player_Action>();

        action?.currentWeapon?.StopSwordWaveCharge();
    }

    public void FootstepSound()
    {
        soundPlayer?.PlayFootstep();
    }

    public void JumpSound()
    {
        soundPlayer?.PlayJump();
    }

    public void LandSound()
    {
        soundPlayer?.PlayLand();
    }

    public void DodgeSound()
    {
        soundPlayer?.PlayDodge();
    }

    public void AttackSound(int attackIndex)
    {
        soundPlayer?.PlayAttack(attackIndex);
    }
}
