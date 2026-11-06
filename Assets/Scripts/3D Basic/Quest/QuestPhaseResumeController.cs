using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum QuestPhasePlayerMode
{
    Party,
    TrialCharacter
}

[System.Serializable]
public class QuestPhaseResumeData
{
    [Header("Phase")]
    public string phaseName;

    [Tooltip("이 Phase가 시작되는 누적 진행도")]
    public int startProgress;

    [Tooltip("이 Phase가 끝나는 누적 진행도")]
    public int endProgress;

    [Header("월드 상태")]
    public List<GameObject> activeObjects = new List<GameObject>();

    [Header("전투")]
    public MonsterSpawner spawner;

    [Header("Quest Ping")]
    public Transform questTarget;

    [Header("플레이 캐릭터")]
    public QuestPhasePlayerMode playerMode;

    public Character_Data trialCharacterData;
}

public class QuestPhaseResumeController : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private string questID;

    [Tooltip("Phase 전투가 들어있는 Quest Step Index")]
    [SerializeField] private int targetStepIndex;

    [SerializeField] private string objectiveID;

    [Header("Phase")]
    [SerializeField]
    private List<QuestPhaseResumeData> phases = new List<QuestPhaseResumeData>();

    [Header("복구 완료 후")]
    [SerializeField] private UnityEvent onRestoreCompleted;

    public static void RestoreAllSequences()
    {
        QuestPhaseResumeController[] controllers = FindObjectsOfType<QuestPhaseResumeController>(true);

        foreach (QuestPhaseResumeController controller in controllers)
        {
            if (controller == null) continue;

            controller.RestoreSequence();
        }
    }

    public void RestoreSequence()
    {
        if (QuestManager.instance == null) return;

        if (!QuestManager.instance.questLog.TryGetValue(questID, out PlayerQuestStatus status)) return;

        // 진행 중인 Quest만 복구
        if (status.status != QuestStatus.IN_PROGRESS) return;

        // 이 시퀀스가 사용되는 Step이 아닐 경우 복구하지 않음
        if (status.currentStepIndex != targetStepIndex) return;

        if (!QuestManager.instance.TryGetCurrentObjectiveProgress(questID, objectiveID, out int currentProgress, out int requiredProgress)) return;

        QuestPhaseResumeData currentPhase = FindPhase(currentProgress);

        if (currentPhase == null)
        {
            Debug.LogWarning($"[QuestPhaseResume] Phase를 찾지 못했습니다. " + $"{questID} / Progress: {currentProgress}");
            return;
        }

        ResetPhaseObjects();
        StopAllPhaseSpawners();

        ApplyPhaseObjects(currentPhase);

        RestorePlayerMode(currentPhase);

        RestoreQuestTarget(currentPhase);

        RestoreSpawner(currentPhase, currentProgress);

        Debug.Log($"[QuestPhaseResume] {questID} 복구 / " + $"Phase: {currentPhase.phaseName} / " + $"Progress: {currentProgress}/{requiredProgress}");

        onRestoreCompleted?.Invoke();
    }

    private QuestPhaseResumeData FindPhase(int currentProgress)
    {
        foreach (QuestPhaseResumeData phase in phases)
        {
            if (phase == null) continue;

            if (currentProgress >= phase.startProgress && currentProgress < phase.endProgress) return phase;
        }

        return null;
    }

    private void ResetPhaseObjects()
    {
        foreach (QuestPhaseResumeData phase in phases)
        {
            if (phase == null || phase.activeObjects == null) continue;

            foreach (GameObject obj in phase.activeObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void StopAllPhaseSpawners()
    {
        foreach (QuestPhaseResumeData phase in phases)
        {
            if (phase == null || phase.spawner == null) continue;

            phase.spawner.StopSpawning();
        }
    }

    private void ApplyPhaseObjects(QuestPhaseResumeData phase)
    {
        if (phase.activeObjects == null) return;

        foreach (GameObject obj in phase.activeObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    private void RestorePlayerMode(QuestPhaseResumeData phase)
    {
        if (BattleManager.instance == null) return;

        if (phase.playerMode == QuestPhasePlayerMode.Party) return;

        if (phase.trialCharacterData == null)
        {
            Debug.LogWarning($"[QuestPhaseResume] " + $"{phase.phaseName}의 Trial Character가 없습니다.");
            return;
        }

        if (!BattleManager.instance.TryGetActiveWorldPose(out Vector3 position, out float rotationY)) return;

        Quaternion rotation = Quaternion.Euler(0f, rotationY, 0f);

        BattleManager.instance.StartTrialCharacter(phase.trialCharacterData, position, rotation);
    }

    private void RestoreQuestTarget(QuestPhaseResumeData phase)
    {
        if (phase.questTarget == null) return;

        QuestManager.instance.SetTrackedQuestTargetOverride(phase.questTarget);
    }

    private void RestoreSpawner(QuestPhaseResumeData phase, int currentProgress)
    {
        if (phase.spawner == null) return;

        int remainingCount = phase.endProgress - currentProgress;

        if (remainingCount <= 0) return;

        phase.spawner.StartFiniteWave(remainingCount);

        Debug.Log($"[QuestPhaseResume] " + $"남은 몬스터 {remainingCount}마리 복구");
    }
}