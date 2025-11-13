using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "NPC/Quest")]
public class Quest : ScriptableObject
{
    [Header("퀘스트 고유 정보")]
    public string questID;      //csv연동용 ID (ex: "Q_001_RatHunter")
    public string questTitle;

    [Header("퀘스트 내용")]
    [TextArea(3, 5)]
    public string description;      //퀘스트 설명

    public List<QuestObjective> objectives;     //이 퀘스트의 목표
    public QuestReward rewards;     //완료 보상
}
