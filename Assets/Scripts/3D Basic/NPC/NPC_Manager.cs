using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCStatus
{
    public string npcID;
    public int currentAffinity;

    // 전투 관련 데이터
    public int level;
    public int currentExp;
    public int maxExp = 100;
    public float[] currentStats;

    // 스탯이 초기화되었는지 확인하는 플래그
    public bool isStatInitialized = false;

    // 생성자: ID만으로 생성 (친밀도용)
    public NPCStatus(string id)
    {
        npcID = id;
        currentAffinity = 0;
        level = 1;
        currentExp = 0;
        currentStats = new float[(int)STAT.STAT_COUNT];
        isStatInitialized = false; // 아직 스탯은 없는 상태
    }

    // 전투 데이터 초기화 (NPC_Data가 들어왔을 때 호출)
    public void InitializeStats(NPC_Data data)
    {
        if (isStatInitialized || data == null) return;

        // 베이스 스탯 복사
        System.Array.Copy(data.baseStats, currentStats, data.baseStats.Length);
        isStatInitialized = true; // 초기화 완료 표시
    }
}

public class NPC_Manager : MonoBehaviour
{
    public static NPC_Manager instance;

    public Dictionary<string, NPCStatus> npcStatusDictionary = new Dictionary<string, NPCStatus>();

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

    // NPC 상태 가져오기 (전투/일상 공용)
    // data가 null이면 친밀도 등 기본 정보만 가져오고,
    // data가 있으면 스탯 초기화 여부를 확인하고 필요시 초기화함
    public NPCStatus GetNPCStatus(string npcID, NPC_Data data = null)
    {
        // 1. 딕셔너리에 없으면 새로 생성 (기본 상태)
        if (!npcStatusDictionary.ContainsKey(npcID))
        {
            npcStatusDictionary[npcID] = new NPCStatus(npcID);
        }

        NPCStatus status = npcStatusDictionary[npcID];

        // 2. 데이터가 제공되었고, 아직 스탯 초기화가 안 되어 있다면 초기화 진행
        // (예: 일상 파트에서 친밀도만 올렸다가, 처음 전투에 나가는 경우)
        if (data != null && !status.isStatInitialized)
        {
            status.InitializeStats(data);
        }

        return status;
    }

    // 경험치 획득 및 레벨업 처리
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

    // 친밀도 가져오기 (NPC_Data 없이 ID만으로 호출 가능)
    public int GetAffinity(string npcID)
    {
        // NPC_Data 없이 호출하므로 스탯 초기화는 안 되지만, 친밀도는 가져올 수 있음
        NPCStatus status = GetNPCStatus(npcID, null);
        return status.currentAffinity;
    }

    // 친밀도 변경
    public void ChangeAffinity(string npcID, int amount)
    {
        if (amount == 0) return;

        NPCStatus status = GetNPCStatus(npcID, null);
        status.currentAffinity += amount;

        Debug.Log($"[{npcID}] 친밀도 변경: {status.currentAffinity - amount} -> {status.currentAffinity} (변동: {amount})");
    }
}