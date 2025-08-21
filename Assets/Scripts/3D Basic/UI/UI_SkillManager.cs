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
            Destroy(gameObject);
    }

    // ADDED: Player_Action에서 호출하여 스킬 슬롯 전체를 초기화하는 메서드
    public void SetupSkillSlots(SkillHolder[] playerSkills)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            // 해당 인덱스의 스킬이 존재하면 슬롯을 설정
            if (i < playerSkills.Length && playerSkills[i] != null)
            {
                skillSlots[i].Setup(playerSkills[i]);
            }
            // 스킬이 없으면(null) 슬롯을 비움
            else
            {
                skillSlots[i].Clear();
            }
        }
    }
}
