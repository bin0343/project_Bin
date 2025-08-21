using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill_Base : ScriptableObject
{
    [Header("Basic Info")]
    public string SkillName;
    public SKILLTYPE skillType;
    public Sprite skillIcon;
    public string animTriggerName;

    [Header("Skill Properties")]
    public float cooldownTime;
    public float mpCost;
    public float duration;

    // REMOVED: lastUseTime, OnEnable, CanUse, Use, ResetSkill 등 상태 관련 코드 모두 제거

    // Use 메서드를 ApplySkillEffects로 변경하여 역할 명확화
    public void ApplySkillEffects(GameObject user)
    {
        var stat = user.GetComponent<Player_Stat>();
        if (stat == null) return;

        // MP 감소
        stat.CurrentMP -= (int)mpCost;

        // 애니메이션 실행
        Animator anim = user.GetComponentInChildren<Animator>();
        if (anim != null && !string.IsNullOrEmpty(animTriggerName))
        {
            anim.SetTrigger(animTriggerName);
        }

        // 실제 스킬 효과 적용 (자식 클래스에서 구현)
        ApplyEffect(user);
    }

    protected abstract void ApplyEffect(GameObject user);
}
