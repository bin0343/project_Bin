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

    [Header("퀘스트 트리거")]
    public Quest questToStart; // 이 선택지를 고르면 시작될 퀘스트
    public Quest questToComplete; // 이 선택지를 고르면 완료(보상)될 퀘스트
}
