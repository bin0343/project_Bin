using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHolder
{
    public Skill_Base SkillData { get; private set; }
    private float lastUseTime;

    public SkillHolder(Skill_Base skillData)
    {
        SkillData = skillData;
        lastUseTime = float.NegativeInfinity; // 생성 시 바로 사용 가능하도록 초기화
    }

    public bool CanUse()
    {
        // 쿨타임 확인
        return Time.time >= lastUseTime + SkillData.cooldownTime;
    }

    public void Use(GameObject user)
    {
        // 실제 스킬 효과는 SkillData에 위임하고, 사용 시간만 기록
        SkillData.ApplySkillEffects(user);
        lastUseTime = Time.time;
    }

    // UI 표시를 위해 남은 쿨타임을 계산하는 함수
    public float GetRemainingCooldown()
    {
        float remaining = (lastUseTime + SkillData.cooldownTime) - Time.time;
        return Mathf.Max(0f, remaining);
    }
}
