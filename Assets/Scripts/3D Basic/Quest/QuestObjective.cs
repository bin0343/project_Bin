using UnityEngine;

public enum ObjectiveType
{
    KILL,       //몬스터 처치
    COLLECT,    //아이템 수집
    TALK_TO,    //특정 npc에게 말걸기
    LOCATION
}

[System.Serializable]
public class QuestObjective
{
    [Header("UI 표시")]
    [TextArea(1, 2)]
    public string displayText;

    public ObjectiveType type;
    public string targetID;     //몬스터 ID (ex : "MON_Rat"), 아이템 ID 등
    public int requiredAmount;
}
