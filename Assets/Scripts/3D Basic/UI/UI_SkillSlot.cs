using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot : MonoBehaviour
{
    public Image SkillIcon;
    public Image CooldownMask;
    public Text CooldownText;
    public Text MpCostText;

    private SkillHolder assignedSkillHolder;

    public bool IsEmpty => assignedSkillHolder == null;

    void Awake()
    {
        Clear();
    }

    public void Setup(SkillHolder skillHolder)
    {
        assignedSkillHolder = skillHolder;
        SkillIcon.sprite = skillHolder.SkillData.skillIcon;
        SkillIcon.enabled = true;
        MpCostText.text = $"{skillHolder.SkillData.mpCost}";
        MpCostText.enabled = true;
        // 초기 쿨타임 UI 업데이트
        UpdateCooldownUI();
    }

    public void Clear()
    {
        assignedSkillHolder = null;
        SkillIcon.sprite = null;
        SkillIcon.enabled = false;
        MpCostText.text = "";
        MpCostText.enabled = false;
        CooldownMask.fillAmount = 0;
        CooldownText.enabled = false;
    }

    // REMOVED: StartCooldown(), GetSkill(), IsOnCooldown() 등 불필요한 메서드 제거

    void Update()
    {
        if (IsEmpty) return;
        UpdateCooldownUI();
    }

    private void UpdateCooldownUI()
    {
        float remaining = assignedSkillHolder.GetRemainingCooldown();
        float totalCooldown = assignedSkillHolder.SkillData.cooldownTime;

        if (remaining > 0)
        {
            CooldownMask.fillAmount = remaining / totalCooldown;
            CooldownText.enabled = true;
            CooldownText.text = remaining.ToString("F1");
        }
        else
        {
            CooldownMask.fillAmount = 0f;
            CooldownText.enabled = false;
        }
    }
}
