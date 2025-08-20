using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SkillManager : MonoBehaviour
{
    public static UI_SkillManager Instance { get; private set; }

    public UI_SkillSlot[] skillSlots;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // 혹시 중복 방지
    }

    public void AssignSkillToSlot(int index, Skill_Base skillData)
    {
        if (index >= 0 && index < skillSlots.Length)
        {
            skillSlots[index].Setup(skillData);
        }
    }

    public void ClearSkillFromSlot(int index)
    {
        if (index >= 0 && index < skillSlots.Length)
        {
            skillSlots[index].Clear();
        }
    }

    public void StartCooldown(int index)
    {
        if (index >= 0 && index < skillSlots.Length)
        {
            skillSlots[index].StartCooldown();
        }
    }
}
