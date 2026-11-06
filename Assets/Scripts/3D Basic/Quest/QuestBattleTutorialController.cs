using UnityEngine;
using UnityEngine.Events;

public class QuestBattleTutorialController : MonoBehaviour
{
    private enum TutorialPhase
    {
        Character1Battle,
        Character2Battle,
        Character3Battle,
        Character1Return,
        Finished
    }

    [Header("Quest")]
    [SerializeField] private string questID = "Q_1001";
    [SerializeField] private string objectiveID;

    [Header("처치 수 기준")]
    [SerializeField] private int character2TriggerCount = 3;
    [SerializeField] private int character3TriggerCount = 6;
    [SerializeField] private int character1ReturnCount = 9;
    [SerializeField] private int finalKillCount = 12;

    [Header("Phase 이벤트")]
    [SerializeField] private UnityEvent onCharacter2Phase;
    [SerializeField] private UnityEvent onCharacter3Phase;
    [SerializeField] private UnityEvent onCharacter1ReturnPhase;
    [SerializeField] private UnityEvent onBattleFinished;

    [Header("전투 목표 위치")]
    [SerializeField] private Transform portalATarget;
    [SerializeField] private Transform portalBTarget;
    [SerializeField] private Transform portalCTarget;

    private TutorialPhase currentPhase = TutorialPhase.Character1Battle;

    private void Start()
    {
        if (QuestManager.instance == null) return;

        QuestManager.instance.OnQuestProgressChanged += HandleQuestProgressChanged;

        SyncCurrentPhase();
    }

    private void OnDestroy()
    {
        if (QuestManager.instance == null) return;

        QuestManager.instance.OnQuestProgressChanged -= HandleQuestProgressChanged;
    }

    private void HandleQuestProgressChanged(PlayerQuestStatus status, Quest quest)
    {
        if (status == null || quest == null) return;

        if (quest.questID != questID) return;

        if (!status.objectiveProgress.TryGetValue(objectiveID, out int killCount)) return;

        ApplyBattleTarget(killCount);
        EvaluatePhase(killCount);
    }

    private void EvaluatePhase(int killCount)
    {
        if (currentPhase == TutorialPhase.Character1Battle && killCount >= character2TriggerCount)
        {
            currentPhase = TutorialPhase.Character2Battle;

            Debug.Log("[전투 튜토리얼] 캐릭터 2 등장 Phase");

            onCharacter2Phase?.Invoke();
            return;
        }

        if (currentPhase == TutorialPhase.Character2Battle && killCount >= character3TriggerCount)
        {
            currentPhase = TutorialPhase.Character3Battle;

            Debug.Log("[전투 튜토리얼] 캐릭터 3 등장 Phase");

            onCharacter3Phase?.Invoke();
            return;
        }

        if (currentPhase == TutorialPhase.Character3Battle && killCount >= character1ReturnCount)
        {
            currentPhase = TutorialPhase.Character1Return;

            Debug.Log("[전투 튜토리얼] 캐릭터 1 복귀 Phase");

            onCharacter1ReturnPhase?.Invoke();
            return;
        }

        if (currentPhase == TutorialPhase.Character1Return && killCount >= finalKillCount)
        {
            currentPhase = TutorialPhase.Finished;

            Debug.Log("[전투 튜토리얼] 최종 전투 완료");

            onBattleFinished?.Invoke();
        }
    }

    public void SyncCurrentPhase()
    {
        if (!QuestManager.instance.TryGetCurrentObjectiveProgress(questID, objectiveID, out int currentAmount, out int requiredAmount)) return;

        if (currentAmount >= finalKillCount)
        {
            currentPhase = TutorialPhase.Finished;
        }
        else if (currentAmount >= character1ReturnCount)
        {
            currentPhase = TutorialPhase.Character1Return;
        }
        else if (currentAmount >= character3TriggerCount)
        {
            currentPhase = TutorialPhase.Character3Battle;
        }
        else if (currentAmount >= character2TriggerCount)
        {
            currentPhase = TutorialPhase.Character2Battle;
        }
        else
        {
            currentPhase = TutorialPhase.Character1Battle;
        }

        ApplyBattleTarget(currentAmount);

        Debug.Log($"[전투 튜토리얼] 진행도 복구 : " + $"{currentAmount}/{requiredAmount}, " + $"Phase = {currentPhase}");
    }

    private void ApplyBattleTarget(int killCount)
    {
        if (QuestManager.instance == null) return;

        Transform target = portalATarget;

        if (killCount >= character1ReturnCount)
        {
            target = portalATarget;
        }
        else if (killCount >= character3TriggerCount)
        {
            target = portalCTarget;
        }
        else if (killCount >= character2TriggerCount)
        {
            target = portalBTarget;
        }

        if (target != null)
        {
            QuestManager.instance.SetTrackedQuestTargetOverride(target);
        }
    }
}