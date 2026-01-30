using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCStatus
{
    public string npcID;
    public int currentAffinity;

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

    public List<NPCStatus> npcStatusList = new List<NPCStatus>();

    public Dictionary<string, NPCStatus> npcStatusDictionary = new Dictionary<string, NPCStatus>();

    public List<string> currentPartyIDs = new List<string>();

    public List<NPC_Data> currentPartyData = new List<NPC_Data>();

    public List<NPC_Data> allNPCDataList;   //모든 npc

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAllNPCs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //모든 npc 등록
    public void InitializeAllNPCs()
    {
        if (allNPCDataList == null) return;

        foreach (var data in allNPCDataList)
        {
            if (data == null) continue;

            // 아직 딕셔너리에 없다면 추가
            if (!npcStatusDictionary.ContainsKey(data.NPCID))
            {
                NPCStatus newStatus = new NPCStatus(data.NPCID);
                newStatus.InitializeStats(data); // 기본 스탯으로 초기화

                npcStatusDictionary.Add(data.NPCID, newStatus);
            }
        }
        Debug.Log($"NPC 매니저 초기화 완료: 총 {npcStatusDictionary.Count}명 등록됨.");
    }

    public void SaveParty(List<string> newPartyIDs, List<NPC_Data> newPartyData)
    {
        currentPartyIDs = new List<string>(newPartyIDs);
        currentPartyData = new List<NPC_Data>(newPartyData); 
    }

    public NPCStatus GetNPCStatus(string npcID, NPC_Data data = null)
    {
        if (!npcStatusDictionary.ContainsKey(npcID))
        {
            NPCStatus newStatus = new NPCStatus(npcID);

            npcStatusDictionary[npcID] = newStatus;

            npcStatusList.Add(newStatus);
        }

        NPCStatus status = npcStatusDictionary[npcID];

        if (data != null && !status.isStatInitialized)
        {
            status.InitializeStats(data);
        }

        return status;
    }

    public bool IsRecruited(string npcID)
    {
        if (!npcStatusDictionary.ContainsKey(npcID)) return false;

        return npcStatusDictionary[npcID].currentAffinity >= 10;
    }

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

    public List<NPCSaveData> GetSaveData()
    {
        List<NPCSaveData> saveList = new List<NPCSaveData>();

        foreach (var kvp in npcStatusDictionary)
        {
            NPCStatus status = kvp.Value;
            NPCSaveData data = new NPCSaveData();

            data.npcID = status.npcID;
            data.affinity = status.currentAffinity;
            data.level = status.level;
            data.currentExp = status.currentExp;

            if (status.currentStats != null)
            {
                data.npcStats = (float[])status.currentStats.Clone();
            }

            saveList.Add(data);
        }
        return saveList;
    }

    public void LoadFromSaveData(List<NPCSaveData> savedList)
    {
        if (savedList == null) return;

        foreach (var data in savedList)
        {
            // 이미 딕셔너리에 있는 NPC라면 값만 갱신
            if (npcStatusDictionary.ContainsKey(data.npcID))
            {
                NPCStatus status = npcStatusDictionary[data.npcID];
                status.currentAffinity = data.affinity;
                status.level = data.level;
                status.currentExp = data.currentExp;

                if (data.npcStats != null && data.npcStats.Length == (int)STAT.STAT_COUNT)
                {
                    status.currentStats = (float[])data.npcStats.Clone();
                    status.isStatInitialized = true; // 스탯이 있으니 초기화된 것으로 처리
                }
                // 레벨에 따른 스탯 재계산 로직이 필요하다면 여기서 호출
                // ex: RecalculateStats(status);
            }
            else
            {
                // (선택사항) 만약 처음 보는 NPC라면 새로 생성해서 넣기
                // 하지만 보통 NPC 기본 데이터는 게임 시작시 초기화되므로 갱신만 하면 됨
            }
        }

        Debug.Log($"NPC {savedList.Count}명 데이터 복구 완료");
    }
}