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

    protected int CalculateSkillDamage(GameObject user, float attackMultiplier, int bonusDamage = 0)
    {
        if (user == null)
        {
            Debug.LogError("[Skill] 스킬 사용자가 없습니다.");
            return 0;
        }

        Character_Stat stat = user.GetComponent<Character_Stat>();

        if (stat == null) stat = user.GetComponentInParent<Character_Stat>();

        if (stat == null) stat = user.GetComponentInChildren<Character_Stat>();

        if (stat == null)
        {
            Debug.LogError("[Skill] Character_Stat을 찾지 못했습니다.");
            return 0;
        }

        int attackPower = stat.attackPower;

        int calculatedDamage = Mathf.RoundToInt(attackPower * Mathf.Max(0f, attackMultiplier));

        calculatedDamage += Mathf.Max(0, bonusDamage);

        calculatedDamage = Mathf.Max(calculatedDamage, 1);

        Debug.Log($"[Skill Damage] 공격력: {attackPower}, " + $"배율: {attackMultiplier}, " + $"추가 데미지: {bonusDamage}, " + $"계산된 공격력: {calculatedDamage}");

        return calculatedDamage;
    }

    protected abstract void ApplyEffect(GameObject user, Vector3 targetPosition);
}
