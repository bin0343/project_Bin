using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    [Header("데이터 베이스")]
    public List<NPC_Data> allNpcData; // 프로젝트의 모든 NPC 데이터를 여기에 등록해두세요.

    [Header("소환 위치")]
    public Transform[] spawnPoints; // 플레이어 주변이나 정해진 위치 (최소 3개)

    void Start()
    {
        SpawnPartyMembers();
    }

    void SpawnPartyMembers()
    {
        // 1. NPC 매니저에서 현재 파티 리스트 가져오기
        List<string> partyIDs = NPC_Manager.instance.currentPartyIDs;

        if (partyIDs == null || partyIDs.Count == 0)
        {
            Debug.Log("편성된 파티원이 없습니다.");
            return;
        }

        // 2. 파티원 수만큼 반복해서 소환
        for (int i = 0; i < partyIDs.Count; i++)
        {
            // 소환 위치가 부족하면 중단 (에러 방지)
            if (i >= spawnPoints.Length) break;

            string id = partyIDs[i];

            // ID로 NPC_Data 찾기
            NPC_Data data = GetNPCDataByID(id);

            if (data != null && data.npcPrefab != null)
            {
                // 3. 프리팹 소환
                GameObject npcObj = Instantiate(data.npcPrefab, spawnPoints[i].position, spawnPoints[i].rotation);

                // 4. [중요] 스탯 초기화 (이 녀석이 누구인지 알려줌)
                NPC_Stat statScript = npcObj.GetComponent<NPC_Stat>();
                if (statScript != null)
                {
                    // 생성 직후 데이터를 주입하고 스탯을 불러오게 함
                    statScript.SetCharacter(data);
                }
            }
        }
    }

    // ID로 데이터를 찾는 헬퍼 함수
    NPC_Data GetNPCDataByID(string id)
    {
        return allNpcData.Find(x => x.NPCID == id);
    }
}