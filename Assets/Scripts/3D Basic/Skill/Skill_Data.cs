using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skill/Skill Data")]
public class Skill_Data : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public SKILLTYPE skillType;
    public Sprite skillIcon;
    public string animTriggerName;

    [Header("Skill Properties")]
    public float cooldownTime;
    public float mpCost;
    public float duration;          // 버프/지속 효과용
    public int attackIncreaseAmount; // 버프 스킬용

    [TextArea]
    public string description;
}
