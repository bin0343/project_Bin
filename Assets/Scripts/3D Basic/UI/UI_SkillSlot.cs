using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot : MonoBehaviour
{
    public Image SkillIcon;               // 스킬 아이콘
    public Image CooldownMask;            // 쿨타임 마스크
    public Text CooldownText;  // 쿨타임 남은 시간
    public Text MpCostText;    // MP 소모량

    private Skill_Base skill;
    private bool isCooldown = false;

    public void Setup(Skill_Base skillData, Sprite icon)
    {
        skill = skillData;
        SkillIcon.sprite = icon;
        MpCostText.text = $"{skill.mpCost}";
        CooldownMask.fillAmount = 0f;
        CooldownText.text = "";
    }

    public void StartCooldown()
    {
        isCooldown = true;
    }

    void Update()
    {
        if (isCooldown && skill != null)
        {
            float remaining = (skill.lastUseTime + skill.cooldownTime) - Time.time;
            if (remaining > 0)
            {
                CooldownMask.fillAmount = remaining / skill.cooldownTime;
                CooldownText.text = remaining.ToString("F1"); // 0.1초 단위 표시
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
