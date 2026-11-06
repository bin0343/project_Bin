using UnityEngine;

public class QuestReturnToPartySequence : MonoBehaviour
{
    [SerializeField] private QuestCutsceneController cutsceneController;

    public void StartSequence()
    {
        if (BattleManager.instance == null)
        {
            Debug.LogWarning("[ReturnSequence] BattleManager가 없습니다.");
            return;
        }

        // Trial 캐릭터 제거 + 원래 파티 캐릭터 복귀
        BattleManager.instance.EndTrialCharacterAndReturnToParty();

        // 그 뒤 복귀 연출
        if (cutsceneController != null)
        {
            cutsceneController.PlayCutscene();
        }
    }
}