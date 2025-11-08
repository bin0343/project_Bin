using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [TextArea(2, 5)]
    public string choiceText;   //플레이어의 선택지

    public int affinityChange;  //선택지 결과(친밀도 상승 or 하락)

    [TextArea(2, 5)]
    public string npcResponse;  //선택지에 따른 npc의 응답

    //public Conversation nextConversation;     //대화 확장시
}
