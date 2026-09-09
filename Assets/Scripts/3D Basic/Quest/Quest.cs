using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "NPC/Quest")]
public class Quest : ScriptableObject
{
    [Header("퀘스트 고유 정보")]
    public string questID;      //(ex: "Q_001_RatHunter")
    public string questTitle;

    [Header("퀘스트 내용")]
    [TextArea(3, 5)] public string description;      //퀘스트 설명
    public string shortDescription;

    [Header("퀘스트 수락 조건")]
    [Tooltip("이 퀘스트를 받기 전에 완료해야 하는 퀘스트")]
    public List<Quest> prerequisiteQuests = new List<Quest>();

    [Header("수락/거절 선택지 설정")]
    public string acceptButtonText = "수락한다";
    public string declineButtonText = "거절한다";

    [Header("선택 후 출력 대사 (단일)")]
    [TextArea(2, 4)] public string acceptedDialogue;
    [TextArea(2, 4)] public string declinedDialogue;

    [Header("퀘스트 전용 대화 (단일 대화형)")]
    [TextArea(2, 4)] public string[] startDialogue;         // 퀘스트 수락 시
    [TextArea(2, 4)] public string[] inProgressDialogue;    // 진행 중일 때
    [TextArea(2, 4)] public string[] completeDialogue;      // 완료 및 보상 수령 시
    [TextArea(2, 4)] public string[] afterCompleteDialogue; // 보상을 다 받은 후 일상 대화

    [Header("퀘스트 목표, 보상")]
    public List<QuestObjective> objectives;     //이 퀘스트의 목표
    public QuestReward rewards;     //완료 보상
}
