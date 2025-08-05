using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill_Base : ScriptableObject
{
    public string SkillName;
    public SKILLTYPE skillType;

    public float cooldownTime;
    public float mpCost;
    public float duration;

    protected float lastUseTime;

    private bool isInitialized = false;

    public virtual bool CanUse(float currentMP)
    {
        if (!isInitialized)
        {
            lastUseTime = -cooldownTime;
            isInitialized = true;
        }

        return Time.time >= lastUseTime + cooldownTime && currentMP >= mpCost;
    }

    public void Use(GameObject user)
    {
        if (!CanUse(user.GetComponent<Player_Stat>().CurrentMp)) return;

        lastUseTime = Time.time;

        // MP 감소
        user.GetComponent<Player_Stat>().CurrentMp -= (int)mpCost;

        // 스킬 효과 적용
        ApplyEffect(user);
    }

    protected abstract void ApplyEffect(GameObject user);
}
