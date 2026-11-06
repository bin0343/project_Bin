using UnityEngine;

public class QuestTrialCharacterSequence : MonoBehaviour
{
    [Header("체험 캐릭터")]
    [SerializeField] private Character_Data trialCharacterData;
    [SerializeField] private Transform spawnPoint;

    [Header("체험 스탯")]
    [SerializeField, Min(1)]
    private int trialLevel = 5;

    [Header("등장 연출")]
    [SerializeField] private QuestCutsceneController cutsceneController;

    public void StartSequence()
    {
        if (cutsceneController != null)
        {
            cutsceneController.PlayCutscene();
        }
    }

    public void ActivateTrialCharacter()
    {
        if (BattleManager.instance == null) return;

        BattleManager.instance.StartTrialCharacter(trialCharacterData, spawnPoint, trialLevel);
    }
}