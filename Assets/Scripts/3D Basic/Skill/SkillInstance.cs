using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillInstance : MonoBehaviour
{
    public Skill_Data data;
    public float lastUseTime = float.NegativeInfinity;

    public SkillInstance(Skill_Data skillData)
    {
        data = skillData;
    }

    public bool CanUse()
    {
        return Time.time >= lastUseTime + data.cooldownTime;
    }

    public void Use(GameObject user)
    {
        if (!CanUse())
        {
            Debug.Log($"[{data.skillName}] 스킬 사용 불가 (쿨타임)");
            return;
        }

        lastUseTime = Time.time;

        Animator anim = user.GetComponentInChildren<Animator>();
        if (anim != null && !string.IsNullOrEmpty(data.animTriggerName))
            anim.SetTrigger(data.animTriggerName);

        ApplyEffect(user);
    }

    private void ApplyEffect(GameObject user)
    {
        var stat = user.GetComponent<Character_Stat>();

        switch (data.skillType)
        {
            case SKILLTYPE.Buff:
                stat.AddEquipmentStat(STAT.Attack, data.attackIncreaseAmount);
                user.GetComponent<MonoBehaviour>().StartCoroutine(RemoveBuffAfterDuration(stat, data.attackIncreaseAmount, data.duration));
                break;

            case SKILLTYPE.Attack:
                break;

            case SKILLTYPE.Heal:
                break;
        }
    }

    private IEnumerator RemoveBuffAfterDuration(Character_Stat stat, int amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        stat.RemoveEquipmentStat(STAT.Attack, amount);
    }
}
