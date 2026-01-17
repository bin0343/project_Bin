using UnityEngine;
using System.Collections.Generic;

public enum NPCClassType { 검술반, 마법반, 궁술반 }

[CreateAssetMenu(fileName = "NPC Data", menuName = "NPC/NPC Data")]
public class NPC_Data : ScriptableObject
{
    [Header("NPC 고유 정보")]
    public string NPCID; 
    public string NPCName;
    public Sprite NPCImage;
    public GameObject npcPrefab;

    [Header("소속 정보")]
    public NPCClassType classType;
    public Sprite standingIllust;

    [Header("전투 설정(기초 스탯)")]
    public float[] baseStats = new float[(int)STAT.STAT_COUNT];

    [Header("성장률(백분율 %)")]
    [Range(0, 100)] 
    public int[] growthRates = new int[(int)STAT.STAT_COUNT];

    [Header("캐릭터 보유 스킬")]
    public List<Skill_Base> npcSkills;  //npc보유 스킬

    [Header("대화 설정")]
    public Conversation startingConversation;

    [Header("퀘스트")]
    public List<Quest> availableQuests;

    [Header("퀘스트 상태별 대화")]
    public Conversation questInProgressConversation;
    public Conversation questCompleteConversation;
}
