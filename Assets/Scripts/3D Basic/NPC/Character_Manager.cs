using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStatus
{
    public string characterID;
    public int currentAffinity;

    public int level;
    public int currentExp;
    public int maxExp = 100;
    public float[] currentStats;

    public bool isStatInitialized = false;

    public CharacterStatus(string id)
    {
        characterID = id;
        currentAffinity = 10;
        level = 1;
        currentExp = 0;
        currentStats = new float[(int)STAT.STAT_COUNT];
        isStatInitialized = false;
    }

    public void InitializeStats(Character_Data data)
    {
        if (isStatInitialized || data == null) return;
        System.Array.Copy(data.baseStats, currentStats, data.baseStats.Length);
        isStatInitialized = true;
    }
}

public class Character_Manager : MonoBehaviour
{
    public static Character_Manager instance;

    public List<CharacterStatus> characterStatusList = new List<CharacterStatus>();

    public Dictionary<string, CharacterStatus> characterStatusDictionary = new Dictionary<string, CharacterStatus>();

    public List<string> currentPartyIDs = new List<string>();

    public List<Character_Data> currentPartyData = new List<Character_Data>();

    public List<Character_Data> allcharacterDataList;   //모든 npc

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
        if (allcharacterDataList == null) return;

        foreach (var data in allcharacterDataList)
        {
            if (data == null) continue;

            // 아직 딕셔너리에 없다면 추가
            if (!characterStatusDictionary.ContainsKey(data.characterID))
            {
                CharacterStatus newStatus = new CharacterStatus(data.characterID);
                newStatus.InitializeStats(data); // 기본 스탯으로 초기화

                characterStatusDictionary.Add(data.characterID, newStatus);
            }
        }
        Debug.Log($"NPC 매니저 초기화 완료: 총 {characterStatusDictionary.Count}명 등록됨.");
    }

    public void SaveParty(List<string> newPartyIDs, List<Character_Data> newPartyData)
    {
        currentPartyIDs = new List<string>(newPartyIDs);
        currentPartyData = new List<Character_Data>(newPartyData); 
    }

    public CharacterStatus GetCharacterStatus(string npcID, Character_Data data = null)
    {
        if (!characterStatusDictionary.ContainsKey(npcID))
        {
            CharacterStatus newStatus = new CharacterStatus(npcID);

            characterStatusDictionary[npcID] = newStatus;

            characterStatusList.Add(newStatus);
        }

        CharacterStatus status = characterStatusDictionary[npcID];

        if (data != null && !status.isStatInitialized)
        {
            status.InitializeStats(data);
        }

        return status;
    }

    public bool IsRecruited(string npcID)
    {
        if (!characterStatusDictionary.ContainsKey(npcID)) return false;

        return characterStatusDictionary[npcID].currentAffinity >= 10;
    }

    // 내가 보유 중인 모든 캐릭터 리스트를 반환
    public List<Character_Data> GetOwnedCharacters()
    {
        List<Character_Data> ownedList = new List<Character_Data>();

        if (allcharacterDataList != null)
        {
            foreach (var data in allcharacterDataList)
            {
                if (data != null && IsRecruited(data.characterID))
                {
                    ownedList.Add(data);
                }
            }
        }

        return ownedList;
    }

    public void SaveParty(List<string> newPartyIDs)
    {
        currentPartyIDs = new List<string>(newPartyIDs);
    }

    public void AddExperience(string npcID, int amount, Character_Data data)
    {
        CharacterStatus status = GetCharacterStatus(npcID, data);
        if (status == null) return;

        status.currentExp += amount;
        while (status.currentExp >= status.maxExp)
        {
            status.currentExp -= status.maxExp;
            LevelUp(status, data);
        }
    }

    private void LevelUp(CharacterStatus status, Character_Data data)
    {
        status.level++;
        status.maxExp *= 2;

        for (int i = 0; i < (int)STAT.STAT_COUNT; i++)
        {
            status.currentStats[i] += data.levelUpStatGains[i];
        }
    }

    public int GetAffinity(string npcID)
    {
        CharacterStatus status = GetCharacterStatus(npcID, null);
        return status.currentAffinity;
    }

    public void ChangeAffinity(string npcID, int amount)
    {
        if (amount == 0) return;

        CharacterStatus status = GetCharacterStatus(npcID, null);
        status.currentAffinity += amount;
    }

    public List<NPCSaveData> GetSaveData()
    {
        List<NPCSaveData> saveList = new List<NPCSaveData>();

        foreach (var kvp in characterStatusDictionary)
        {
            CharacterStatus status = kvp.Value;
            NPCSaveData data = new NPCSaveData();

            data.npcID = status.characterID;
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
            if (characterStatusDictionary.ContainsKey(data.npcID))
            {
                CharacterStatus status = characterStatusDictionary[data.npcID];
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