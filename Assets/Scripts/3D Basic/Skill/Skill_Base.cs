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
    public float mpCost;
    public float duration;

    [Header("Casting")]
    public float castTime = 0.5f;

    public void ApplySkillEffects(GameObject user)
    {
        var playerStat = user.GetComponent<Player_Stat>();
        var npcStat = user.GetComponent <NPC_Stat>();

        if (playerStat != null)
        {
            if (playerStat.currentMP < mpCost) return;
            playerStat.currentMP -= (int)mpCost;
        }
        else if (npcStat != null)
        {
            if (npcStat.currentMP < mpCost) return;
            npcStat.currentMP -= (int)mpCost;
        }

        Animator anim = user.GetComponentInChildren<Animator>();
        if (anim != null && !string.IsNullOrEmpty(animTriggerName))
        {
            anim.SetTrigger(animTriggerName);
        }

        ApplyEffect(user);
    }

    protected abstract void ApplyEffect(GameObject user);
}
