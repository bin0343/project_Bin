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

        if (!CanUse(user.GetComponent<Player_Stat>().CurrentMP))
        {
            Debug.Log("Cannot Use Skill Yet");
            return;
        }

        lastUseTime = Time.time;
        Debug.Log("Skill Used");

        // MP 감소
        user.GetComponent<Player_Stat>().CurrentMP -= (int)mpCost;

        // 스킬 효과 적용
        ApplyEffect(user);
    }

    public void ResetSkill()
    {
        lastUseTime = float.NegativeInfinity;
    }

    protected abstract void ApplyEffect(GameObject user);
}
