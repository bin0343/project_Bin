using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestStep
{
    [Header("단계 정보")]
    public string stepTitle;

    [TextArea(2, 4)]
    public string stepDescription;

    [Header("이 단계의 목표")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("단계 완료 대사")]
    public string completeSpeakerName;

    [TextArea(2, 4)]
    public string[] completeDialogue;
}