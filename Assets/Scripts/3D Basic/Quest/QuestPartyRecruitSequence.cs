using System.Collections.Generic;
using UnityEngine;

public class QuestPartyRecruitSequence : MonoBehaviour
{
    [Header("정식 영입 캐릭터")]
    [SerializeField] private Character_Data character2;
    [SerializeField] private Character_Data character3;

    public void RecruitStoryParty()
    {
        if (Character_Manager.Instance == null)
        {
            Debug.LogWarning("[QuestRecruit] Character_Manager가 없습니다.");
            return;
        }

        if (character2 == null || character3 == null)
        {
            Debug.LogWarning("[QuestRecruit] 영입할 Character_Data가 없습니다.");
            return;
        }

        Character_Manager manager = Character_Manager.Instance;

        bool character2WasOwned = manager.IsRecruited(character2.characterID);
        bool character3WasOwned = manager.IsRecruited(character3.characterID);

        manager.RecruitCharacter(character2.characterID);
        manager.RecruitCharacter(character3.characterID);

        if (character2WasOwned && character3WasOwned)
        {
            Debug.Log("[QuestRecruit] 이미 영입된 캐릭터입니다. " + "현재 파티 구성은 유지합니다.");

            return;
        }

        List<Character_Data> newPartyData = new List<Character_Data>();

        List<string> newPartyIDs = new List<string>();

        if (manager.currentPartyData != null && manager.currentPartyData.Count > 0 && manager.currentPartyData[0] != null)
        {
            Character_Data firstCharacter = manager.currentPartyData[0];

            newPartyData.Add(firstCharacter);
            newPartyIDs.Add(firstCharacter.characterID);
        }

        // 2번 캐릭터
        newPartyData.Add(character2);
        newPartyIDs.Add(character2.characterID);

        // 3번 캐릭터
        newPartyData.Add(character3);
        newPartyIDs.Add(character3.characterID);

        // 3. 파티 데이터 저장
        manager.SaveParty(newPartyIDs, newPartyData);

        // 4. 실제 전투용 캐릭터 다시 생성
        if (BattleManager.instance != null)
        {
            BattleManager.instance.InitializeParty();
        }

        Debug.Log($"[QuestRecruit] 영입 완료 : " + $"{character2.characterName}, " + $"{character3.characterName}");
    }
}