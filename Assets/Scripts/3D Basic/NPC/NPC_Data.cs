using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPC Data", menuName = "NPC/NPC Data")]
public class NPC_Data : ScriptableObject
{
    [Header("NPC 고유 정보")]
    public string NPCID;       //string? int?
    public string NPCName;
    public Sprite NPCImage;

    [Header("대화 설정")]
    public Conversation startingConversation;

    [Header("퀘스트")]
    public List<Quest> availableQuests; // 이 NPC가 주는 퀘스트 목록

    [Header("퀘스트 상태별 대화")]
    public Conversation questInProgressConversation; // 퀘스트 진행 중일 때 대화
    public Conversation questCompleteConversation; // 퀘스트 완료 (보상 전) 대화
}
