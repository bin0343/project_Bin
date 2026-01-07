using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueEntry
{
    public int id;
    public string speakerName;
    public string dialogueText;
    public List<ChoiceData> choices = new List<ChoiceData>();
}

[System.Serializable]
public class ChoiceData
{
    public string text;         // 선택지 텍스트
    public string nextID;       // 다음 대화 ID (숫자 혹은 "EXIT")
    public string effectType;   // 효과 타입 (예: AFFINITY, NPC_STR)
    public string effectValue;     // 효과 수치
}