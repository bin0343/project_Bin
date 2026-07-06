using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill_Base : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public SKILLTYPE skillType;
    public Sprite skillIcon;
    public string animTriggerName;

    [Header("Skill Properties")]
    public float cooldownTime;
    public float duration;

    [Header("Casting")]
    public float castTime = 0.5f;

    public void ApplySkillEffects(GameObject user)
    {
        ApplyEffect(user, user.transform.position + user.transform.forward);
    }

    public void ApplySkillEffects(GameObject user, Vector3 targetPosition)
    {
        ApplyEffect(user, targetPosition);
    }

    protected abstract void ApplyEffect(GameObject user, Vector3 targetPosition);
}
