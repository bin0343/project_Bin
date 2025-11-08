using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Conversation", menuName = "NPC/Conversation")]
public class Conversation : ScriptableObject
{
    [TextArea(2, 5)]
    public string npcLine;  //npc가 먼저 하는 말(질문)

    public List<DialogueChoice> playerChoice;
}
