using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [TextArea(2, 5)]
    public string choiceText; 

    public int affinityChange; 

    [TextArea(2, 5)]
    public string npcResponse;

    [Header("이후 동작 설정")]
    public bool openInteractionMenu = false;    //체크하면 대화 하고 메뉴판이 열리게

    //public Conversation nextConversation;     //대화 확장시

    [Header("퀘스트 트리거")]
    public Quest questToStart;
    public Quest questToComplete;
}
