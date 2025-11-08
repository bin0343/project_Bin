using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCStatus      //(npc의 현재 상태: 친밀도)
{
    public string npcID;
    public int currentAffinity;

    public NPCStatus(string id)
    {
        npcID = id;
        currentAffinity = 0; // 기본 친밀도
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
        DontDestroyOnLoad(gameObject);  //게임 매니저 역할
    }

    public int GetAffinity(string npcID)    //npc의 친밀도 가져오기(없으면 새로 생성)
    {
        if (!npcStatusDictionary.ContainsKey(npcID))
        {
            npcStatusDictionary[npcID] = new NPCStatus(npcID);
        }
        return npcStatusDictionary[npcID].currentAffinity;
    }

    public void ChangeAffinity(string npcID, int amount)    //친밀도 변경
    {
        if (amount == 0) return;

        int currentAffinity = GetAffinity(npcID);    //없으면 0으로 자동 생성
        npcStatusDictionary[npcID].currentAffinity += amount;

        Debug.Log($"[{npcID}] 친밀도 변경: {currentAffinity} -> {npcStatusDictionary[npcID].currentAffinity} (변동: {amount})");
    }
}
