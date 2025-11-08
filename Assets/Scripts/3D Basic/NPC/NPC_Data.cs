using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NPC Data", menuName = "NPC/NPC Data")]
public class NPC_Data : ScriptableObject
{
    [Header("NPC 고유 정보")]
    public string NPCID;       //string? int?
    public string NPCName;
    public Sprite NPCImage;

    [Header("대화 설정")]
    public Conversation startingConversation;
}
