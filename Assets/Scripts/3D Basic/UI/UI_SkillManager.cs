using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SkillManager : MonoBehaviour
{
    public UI_SkillManager Instance;

    public UI_SkillSlot[] skillSlots;   // 하위 슬롯들
    public Sprite[] skillIcons;         // 스킬 아이콘
    public Skill_Base[] skills;         // 실제 스킬 데이터 (ScriptableObject)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 슬롯 초기 세팅
        for (int i = 0; i < skills.Length; i++)
        {
            skillSlots[i].Setup(skills[i], skillIcons[i]);
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
