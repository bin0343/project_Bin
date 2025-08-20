using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill_Base : ScriptableObject
{
    [Header("Basic Info")]
    public string SkillName;                  // 스킬 이름
    public SKILLTYPE skillType;               // 스킬 타입 (예: 공격, 버프 등)
    public Sprite skillIcon;                  // 스킬 아이콘 (UI에서 표시할 이미지)
    public string animTriggerName;            // 스킬 사용 시 애니메이션 트리거 이름

    [Header("Skill Properties")]
    public float cooldownTime;                // 쿨타임
    public float mpCost;                      // MP 소모량
    public float duration;                    // 지속 시간 (버프나 DOT 등)

    [HideInInspector] public float lastUseTime;

    protected virtual void OnEnable()
    {
        lastUseTime = float.NegativeInfinity;
    }

    public virtual bool CanUse(float currentMP)
    {
        return Time.time >= lastUseTime + cooldownTime && currentMP >= mpCost;
    }

    public void Use(GameObject user)
    {
        Debug.Log($"Skill Use Called at {Time.time}, Last Use: {lastUseTime}");

        var stat = user.GetComponent<Player_Stat>();
        if (!CanUse(stat.CurrentMP))
        {
            Debug.Log("Cannot Use Skill Yet");
            return;
        }

        lastUseTime = Time.time;
        Debug.Log("Skill Used");

        // MP 감소
        stat.CurrentMP -= (int)mpCost;

        // 애니메이션 실행 (animTriggerName 이 비어있지 않은 경우)
        Animator anim = user.GetComponentInChildren<Animator>();
        if (anim != null && !string.IsNullOrEmpty(animTriggerName))
        {
            anim.SetTrigger(animTriggerName);
        }

        // 스킬 효과 적용
        ApplyEffect(user);
    }

    public void ResetSkill()
    {
        lastUseTime = float.NegativeInfinity;
    }

    protected abstract void ApplyEffect(GameObject user);
}
