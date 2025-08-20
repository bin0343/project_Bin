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

    public bool CanUse(float currentMP)
    {
        return Time.time >= lastUseTime + data.cooldownTime && currentMP >= data.mpCost;
    }

    public void Use(GameObject user)
    {
        var stat = user.GetComponent<Player_Stat>();
        if (!CanUse(stat.CurrentMP))
        {
            Debug.Log($"[{data.skillName}] 스킬 사용 불가 (쿨타임 or MP 부족)");
            return;
        }

        lastUseTime = Time.time;
        stat.CurrentMP -= (int)data.mpCost;

        Animator anim = user.GetComponentInChildren<Animator>();
        if (anim != null && !string.IsNullOrEmpty(data.animTriggerName))
            anim.SetTrigger(data.animTriggerName);

        ApplyEffect(user);
    }

    private void ApplyEffect(GameObject user)
    {
        var stat = user.GetComponent<Player_Stat>();

        switch (data.skillType)
        {
            case SKILLTYPE.Buff:
                stat.AttackPower += data.attackIncreaseAmount;
                user.GetComponent<MonoBehaviour>().StartCoroutine(RemoveBuffAfterDuration(stat, data.attackIncreaseAmount, data.duration));
                break;

            case SKILLTYPE.Attack:
                break;

            case SKILLTYPE.Heal:
                break;
        }
    }

    private IEnumerator RemoveBuffAfterDuration(Player_Stat stat, int amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        stat.AttackPower -= amount;
    }
}
