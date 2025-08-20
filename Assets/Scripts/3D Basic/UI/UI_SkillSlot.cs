using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot : MonoBehaviour
{
    [Header("Skill Data (Drag & Drop in Inspector)")]
    public Skill_Base assignedSkill;

    public Image SkillIcon;               // 스킬 아이콘
    public Image CooldownMask;            // 쿨타임 마스크
    public Text CooldownText;  // 쿨타임 남은 시간
    public Text MpCostText;    // MP 소모량

    private Skill_Base skill;
    private bool isCooldown = false;

    public bool IsEmpty => skill == null;  // 빈 슬롯 여부 체크

    void Awake()
    {
        SkillIcon.enabled = false;
        CooldownMask.fillAmount = 0f;

        CooldownText.text = "";
        CooldownText.enabled = false;

        MpCostText.text = "";
        MpCostText.enabled = false;

        if (assignedSkill != null)
            Setup(assignedSkill);
    }

    public void Setup(Skill_Base skillData)
    {
        assignedSkill = skillData;
        SkillIcon.sprite = skillData.skillIcon;
        SkillIcon.enabled = true;
        MpCostText.text = $"{skillData.mpCost}";
        MpCostText.enabled = true;
        CooldownMask.fillAmount = 0f;
        CooldownText.text = "";
    }

    public void Clear()
    {
        skill = null;
        SkillIcon.sprite = null;
        SkillIcon.enabled = false;
        MpCostText.text = "";
        MpCostText.enabled = false;
        CooldownMask.fillAmount = 0f;
        CooldownText.text = "";
        CooldownText.enabled = false;
    }

    public void StartCooldown()
    {
        if (assignedSkill == null) return;
        isCooldown = true;
        assignedSkill.lastUseTime = Time.time;
    }

    public Skill_Base GetSkill()
    {
        return assignedSkill;
    }


    public bool IsOnCooldown()     // 쿨타임 여부 체크
    {
        if (skill == null) return false;
        return (Time.time < skill.lastUseTime + skill.cooldownTime);
    }

    void Update()
    {
        if (isCooldown && assignedSkill != null)
        {
            float remaining = (assignedSkill.lastUseTime + assignedSkill.cooldownTime) - Time.time;
            if (remaining > 0)
            {
                CooldownMask.fillAmount = remaining / assignedSkill.cooldownTime;
                CooldownText.text = remaining.ToString("F1");
            }
            else
            {
                CooldownMask.fillAmount = 0f;
                CooldownText.text = "";
                isCooldown = false;
            }
        }
    }
}
