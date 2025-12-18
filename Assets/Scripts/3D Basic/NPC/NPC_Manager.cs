using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCStatus
{
    public string npcID;
    public int currentAffinity; // 인스펙터에서 수정 가능해짐

    // 전투 관련 데이터
    public int level;
    public int currentExp;
    public int maxExp = 100;
    public float[] currentStats;

    public bool isStatInitialized = false;

    public NPCStatus(string id)
    {
        npcID = id;
        currentAffinity = 10;
        level = 1;
        currentExp = 0;
        currentStats = new float[(int)STAT.STAT_COUNT];
        isStatInitialized = false;
    }

    public void InitializeStats(NPC_Data data)
    {
        if (isStatInitialized || data == null) return;
        System.Array.Copy(data.baseStats, currentStats, data.baseStats.Length);
        isStatInitialized = true;
    }
}

public class NPC_Manager : MonoBehaviour
{
    public static NPC_Manager instance;

    // [핵심] 인스펙터용 리스트 추가!
    // Dictionary는 인스펙터에 안 보이지만, List는 보입니다.
    // NPCStatus가 클래스(Class)이므로, 리스트 값을 바꾸면 딕셔너리 값도 같이 바뀝니다.
    public List<NPCStatus> npcStatusList = new List<NPCStatus>();

    public Dictionary<string, NPCStatus> npcStatusDictionary = new Dictionary<string, NPCStatus>();

    // [추가] 동아리 편성용 파티 리스트
    public List<string> currentPartyIDs = new List<string>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public NPCStatus GetNPCStatus(string npcID, NPC_Data data = null)
    {
        // 1. 딕셔너리에 없으면 새로 생성
        if (!npcStatusDictionary.ContainsKey(npcID))
        {
            NPCStatus newStatus = new NPCStatus(npcID);

            // 딕셔너리에 등록
            npcStatusDictionary[npcID] = newStatus;

            // [핵심] 인스펙터용 리스트에도 등록 (서로 같은 객체를 바라봄)
            npcStatusList.Add(newStatus);
        }

        NPCStatus status = npcStatusDictionary[npcID];

        if (data != null && !status.isStatInitialized)
        {
            status.InitializeStats(data);
        }

        return status;
    }

    // 영입 조건 확인 함수 (동아리 편성 때 사용)
    public bool IsRecruited(string npcID)
    {
        if (!npcStatusDictionary.ContainsKey(npcID)) return false;

        // 예: 친밀도 10 이상이면 영입된 것으로 간주
        return npcStatusDictionary[npcID].currentAffinity >= 10;
    }

    // 파티 저장
    public void SaveParty(List<string> newPartyIDs)
    {
        currentPartyIDs = new List<string>(newPartyIDs);
    }

    public void AddExperience(string npcID, int amount, NPC_Data data)
    {
        NPCStatus status = GetNPCStatus(npcID, data);
        if (status == null) return;

        status.currentExp += amount;
        while (status.currentExp >= status.maxExp)
        {
            status.currentExp -= status.maxExp;
            LevelUp(status, data);
        }
    }

    private void LevelUp(NPCStatus status, NPC_Data data)
    {
        status.level++;
        Debug.Log($"<b>{data.NPCName}</b> 레벨 업! (Lv.{status.level})");

        for (int i = 0; i < (int)STAT.STAT_COUNT; i++)
        {
            int roll = Random.Range(0, 100);
            if (roll < data.growthRates[i])
            {
                status.currentStats[i] += 1;
            }
        }
    }

    public int GetAffinity(string npcID)
    {
        NPCStatus status = GetNPCStatus(npcID, null);
        return status.currentAffinity;
    }

    public void ChangeAffinity(string npcID, int amount)
    {
        if (amount == 0) return;

        NPCStatus status = GetNPCStatus(npcID, null);
        status.currentAffinity += amount;

        Debug.Log($"[{npcID}] 친밀도 변경: {status.currentAffinity - amount} -> {status.currentAffinity}");
    }
}