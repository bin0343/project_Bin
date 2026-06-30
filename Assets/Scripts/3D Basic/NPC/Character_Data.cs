using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Character Data", menuName = "Character/Character Data")]
public class Character_Data : ScriptableObject
{
    [Header("NPC 고유 정보")]
    public string characterID; 
    public string characterName;
    public Sprite characterImage;
    public Sprite characterPortrait;
    [Tooltip("실제 전투 맵에 소환될 프리팹 (스크립트, 중력 포함)")]
    public GameObject characterPrefab;
    [Tooltip("캐릭터 정보창에 띄울 가벼운 껍데기 프리팹 (애니메이터, 모델링만 포함)")]
    public GameObject uiPrefab;

    [Header("UI 프리뷰 전용 개별 보정 스탯")]
    public float uiPreviewScale = 1.0f;
    public float uiPreviewYOffset = 0.0f;

    [Header("전투 설정(기초 스탯)")]
    public float[] baseStats = new float[(int)STAT.STAT_COUNT];

    [Header("레벨업 시 스탯 증가량 (고정치)")]
    public float[] levelUpStatGains = new float[(int)STAT.STAT_COUNT];

    [Header("캐릭터 보유 스킬")]
    public List<Skill_Base> characterSkills;  //npc보유 스킬

    /*[Header("대화 설정")]
    public Conversation startingConversation;

    [Header("퀘스트")]
    public List<Quest> availableQuests;

    [Header("퀘스트 상태별 대화")]
    public Conversation questInProgressConversation;
    public Conversation questCompleteConversation;*/
}
