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
}