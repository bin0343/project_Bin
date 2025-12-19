using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    [Header("소환 위치")]
    public Vector3[] spawnOffsets = new Vector3[]
    {
        new Vector3(-2, 0, 1),  // 1번 동료: 왼쪽 뒤
        new Vector3(-2, 0, -1), // 2번 동료: 오른쪽 뒤
        new Vector3(-3, 0, 0)   // 3번 동료: 더 뒤쪽
    };

    void Start()
    {
        SpawnPartyMembers();
    }

    void SpawnPartyMembers()
    {
        if (NPC_Manager.instance == null) return;

        List<NPC_Data> partyData = NPC_Manager.instance.currentPartyData;

        if (partyData == null || partyData.Count == 0)
        {
            Debug.Log("편성된 파티원이 없습니다.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("씬에 Player 태그가 달린 오브젝트가 없습니다! 먼저 배치해주세요.");
            return;
        }
        Transform playerTr = player.transform;

        for (int i = 0; i < partyData.Count; i++)
        {
            if (i >= spawnOffsets.Length) break;

            NPC_Data data = partyData[i];

            if (data != null && data.npcPrefab != null)
            {
                Vector3 spawnPos = playerTr.TransformPoint(spawnOffsets[i]);

                spawnPos.y = playerTr.position.y;

                GameObject npcObj = Instantiate(data.npcPrefab, spawnPos, playerTr.rotation);

                var stat = npcObj.GetComponent<NPC_Stat>();
                if (stat != null)
                {
                    stat.SetCharacter(data);
                }
            }
        }
    }
}