using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PartyPreset
{
    public string presetName;
    public List<string> characterIDs = new List<string>();
}

public class CharacterStatus
{
    public string characterID;

    public int level;
    public int currentExp;
    public int ascensionStage;
    public int maxExp = 100;
    public float[] currentStats;

    public bool isStatInitialized = false;
    public bool isOwned = false;       // 스토리 진행 중 영입 완료 여부
    public ItemHolder equippedWeapon;

    public CharacterStatus(string id)
    {
        characterID = id;
        level = 1;
        currentExp = 0;
        maxExp = 100;
        ascensionStage = 0;
        currentStats = new float[(int)STAT.STAT_COUNT];
        isStatInitialized = false;
        isOwned = false;
        equippedWeapon = null;
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
    public static Character_Manager Instance;

    public List<CharacterStatus> characterStatusList = new List<CharacterStatus>();
    public List<string> currentPartyIDs = new List<string>();
    public List<Character_Data> currentPartyData = new List<Character_Data>();
    public List<Character_Data> allcharacterDataList;   //모든 npc
    public List<PartyPreset> partyPresets = new List<PartyPreset>();
    public Dictionary<string, CharacterStatus> characterStatusDictionary = new Dictionary<string, CharacterStatus>();

    [Header("캐릭터 레벨 및 돌파")]
    [Tooltip("돌파 한 단계마다 열리는 레벨 구간")]
    [SerializeField, Min(1)]
    private int levelCapStep = 20;
    [Tooltip("캐릭터가 도달할 수 있는 최종 최대 레벨")]
    [SerializeField, Min(1)]
    private int absoluteMaxLevel = 80;
    [Tooltip("다음 레벨 요구 경험치 배율. 1이면 모든 레벨의 요구 경험치가 동일")]
    [SerializeField, Min(0.01f)]
    private float expRequirementMultiplier = 1f;

    [Header("시작 시 기본 지급할 캐릭터 목록")]
    [Tooltip("게임 시작 시 자동으로 보유 상태로 만들어줄 캐릭터들의 ID")]
    public List<string> defaultOwnedCharacterIDs = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;

        InitializeAllNPCs();
    }

    //모든 npc 등록
    public void InitializeAllNPCs()
    {
        if (allcharacterDataList == null) return;

        foreach (var data in allcharacterDataList)
        {
            if (data == null) continue;

            if (!characterStatusDictionary.ContainsKey(data.characterID))
            {
                CharacterStatus newStatus = new CharacterStatus(data.characterID);
                newStatus.InitializeStats(data); // 기본 스탯으로 초기화

                RecalculateStatsByLevel(newStatus, data);

                if (defaultOwnedCharacterIDs != null && defaultOwnedCharacterIDs.Contains(data.characterID))
                {
                    newStatus.isOwned = true;
                }

                if (data.characterPrefab != null)
                {
                    Player_Equipment pEquip = data.characterPrefab.GetComponent<Player_Equipment>();
                    if (pEquip != null && pEquip.equipmentSlots != null && pEquip.equipmentSlots.Length > 0)
                    {
                        if (pEquip.equipmentSlots[0] != null && pEquip.equipmentSlots[0].ItemData != null)
                        {
                            newStatus.equippedWeapon = pEquip.equipmentSlots[0];
                        }
                    }
                }

                characterStatusDictionary.Add(data.characterID, newStatus);
                characterStatusList.Add(newStatus);
            }
        }
        Debug.Log($"NPC 매니저 초기화 완료: 총 {characterStatusDictionary.Count}명 등록됨.");
    }

    public void RecruitCharacter(string npcID)
    {
        if (characterStatusDictionary.ContainsKey(npcID))
        {
            if (!characterStatusDictionary[npcID].isOwned)
            {
                characterStatusDictionary[npcID].isOwned = true;
            }
        }
    }

    public bool IsRecruited(string npcID)
    {
        if (!characterStatusDictionary.ContainsKey(npcID)) return false;

        return characterStatusDictionary[npcID].isOwned;
    }

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

    #region 파티 & 프리셋
    public void SaveParty(List<string> newPartyIDs, List<Character_Data> newPartyData)
    {
        currentPartyIDs = new List<string>(newPartyIDs);
        currentPartyData = new List<Character_Data>(newPartyData); 
    }

    public void SaveParty(List<string> newPartyIDs)
    {
        currentPartyIDs = new List<string>(newPartyIDs);
    }

    public void SavePreset(int index, string name, List<string> ids)
    {
        if (partyPresets.Count <= index)
        {
            for (int i = partyPresets.Count; i <= index; i++)
            {
                partyPresets.Add(new PartyPreset());
            }
        }
        partyPresets[index].presetName = name;
        partyPresets[index].characterIDs = new List<string>(ids);
    }

    public PartyPreset GetPreset(int index)
    {
        if (index < 0 || index >= partyPresets.Count) return null;
        return partyPresets[index];
    }
    #endregion

    #region 레벨 & 경험치
    public int AddExperience(string npcID, int amount, Character_Data data)
    {
        CharacterStatus status = GetCharacterStatus(npcID, data);
        if (status == null || data == null) return 0;

        if (amount <= 0) return 0;

        int levelCap = GetCurrentLevelCap(status);

        if (status.level >= levelCap)
        {
            status.level = levelCap;
            status.currentExp = 0;

            return 0;
        }

        int requiredExpToCap = GetRequiredExpToCurrentCap(status);

        int appliedExp = Mathf.Min(amount, requiredExpToCap);

        status.currentExp += appliedExp;

        while (status.level < levelCap && status.currentExp >= status.maxExp)
        {
            status.currentExp -= status.maxExp;

            LevelUp(status, data);
        }

        if (status.level >= levelCap)
        {
            status.level = levelCap;
            status.currentExp = 0;
        }

        return appliedExp;
    }

    public int GetRequiredExpToCurrentCap(CharacterStatus status)
    {
        if (status == null) return 0;

        int levelCap = GetCurrentLevelCap(status);

        if (status.level >= levelCap) return 0;

        int tempLevel = status.level;
        int tempExp = status.currentExp;
        int tempRequiredExp = Mathf.Max(status.maxExp, 1);

        long totalRequiredExp = 0;

        while (tempLevel < levelCap)
        {
            int remainingForNextLevel = Mathf.Max(tempRequiredExp - tempExp, 0);

            totalRequiredExp += remainingForNextLevel;

            tempLevel++;
            tempExp = 0;

            if (tempLevel < levelCap)
            {
                tempRequiredExp = GetNextRequiredExp(tempRequiredExp);
            }
        }

        if (totalRequiredExp > int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)totalRequiredExp;
    }

    private void LevelUp(CharacterStatus status, Character_Data data)
    {
        if (status == null || data == null) return;

        int levelCap = GetCurrentLevelCap(status);

        if (status.level >= levelCap) return;

        status.level++;
        status.maxExp = GetNextRequiredExp(status.maxExp);

        RecalculateStatsByLevel(status, data);
    }

    //캐릭터 레벨 별 스탯
    public void RecalculateStatsByLevel(CharacterStatus status, Character_Data data)
    {
        if (status == null || data == null) return;

        int statCount = (int)STAT.STAT_COUNT;

        if (status.currentStats == null || status.currentStats.Length != statCount)
        {
            status.currentStats = new float[statCount];
        }

        int levelIncreaseCount = Mathf.Max(status.level - 1, 0);

        for (int i = 0; i < statCount; i++)
        {
            float level1Stat = data.baseStats[i];

            float growthPerLevel = data.levelUpStatGains[i];

            status.currentStats[i] = level1Stat + growthPerLevel * levelIncreaseCount;
        }

        status.isStatInitialized = true;
    }

    public int GetNextRequiredExp(int currentRequiredExp)
    {
        return Mathf.Max(1, Mathf.RoundToInt(currentRequiredExp * expRequirementMultiplier));
    }

    public int GetCurrentLevelCap(CharacterStatus status)
    {
        if (status == null)
        {
            return levelCapStep;
        }

        int stage = Mathf.Max(status.ascensionStage, 0);

        int calculatedCap = (stage + 1) * levelCapStep;

        return Mathf.Min(calculatedCap, absoluteMaxLevel);
    }

    public void SimulateExperience(CharacterStatus status, int requestedExp, out int previewLevel, out int previewExp, out int previewRequiredExp, out int appliedExp)
    {
        previewLevel = status.level;
        previewExp = status.currentExp;
        previewRequiredExp = Mathf.Max(status.maxExp, 1);
        appliedExp = 0;

        int levelCap = GetCurrentLevelCap(status);

        // 이미 현재 돌파 상한이라면 경험치 바를 가득 찬 상태로 표시
        if (previewLevel >= levelCap)
        {
            previewLevel = levelCap;
            previewExp = previewRequiredExp;

            return;
        }

        int requiredToCap = GetRequiredExpToCurrentCap(status);

        appliedExp = Mathf.Min(Mathf.Max(requestedExp, 0), requiredToCap);

        previewExp += appliedExp;

        while (previewLevel < levelCap && previewExp >= previewRequiredExp)
        {
            previewExp -= previewRequiredExp;
            previewLevel++;

            // 아직 상한이 아닐 때만 다음 레벨 요구 경험치로 변경
            if (previewLevel < levelCap)
            {
                previewRequiredExp = GetNextRequiredExp(previewRequiredExp);
            }
        }

        // 상한에 도달하면 가득 찬 경험치 바로 표시
        if (previewLevel >= levelCap)
        {
            previewLevel = levelCap;
            previewExp = previewRequiredExp;
        }
    }

    #endregion

    #region 캐릭터 돌파
    //돌파 함수
    public bool IsAtCurrentLevelCap(CharacterStatus status)
    {
        if (status == null) return false;

        int currentLevelCap = GetCurrentLevelCap(status);

        return status.level >= currentLevelCap;
    }

    //현재 캐릭터가 돌파 가능한지
    public bool CanAscend(CharacterStatus status)
    {
        if (status == null) return false;

        int currentLevelCap = GetCurrentLevelCap(status);

        bool reachedCurrentCap = status.level >= currentLevelCap;

        bool hasNextAscension = currentLevelCap < absoluteMaxLevel;

        return reachedCurrentCap && hasNextAscension;
    }

    //돌파 후 상한 계산
    public int GetNextLevelCap(CharacterStatus status)
    {
        if (status == null) return levelCapStep;

        int currentLevelCap = GetCurrentLevelCap(status);

        return Mathf.Min(currentLevelCap + levelCapStep, absoluteMaxLevel);
    }

    //돌파 메서드
    public bool AscendCharacter(string characterID, Character_Data data)
    {
        if (data == null) return false;

        CharacterStatus status = GetCharacterStatus(characterID, data);

        if (!CanAscend(status)) return false;

        status.ascensionStage++;
        status.currentExp = 0;

        RecalculateStatsByLevel(status, data);

        return true;
    }
    #endregion

    #region Save & Load
    public List<NPCSaveData> GetSaveData()
    {
        List<NPCSaveData> saveList = new List<NPCSaveData>();

        foreach (var kvp in characterStatusDictionary)
        {
            CharacterStatus status = kvp.Value;
            NPCSaveData data = new NPCSaveData();

            data.npcID = status.characterID;
            data.affinity = status.isOwned ? 1 : 0;
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
                status.isOwned = (data.affinity == 1);
                status.level = data.level;
                status.currentExp = data.currentExp;

                if (data.npcStats != null && data.npcStats.Length == (int)STAT.STAT_COUNT)
                {
                    status.currentStats = (float[])data.npcStats.Clone();
                    status.isStatInitialized = true; // 스탯이 있으니 초기화된 것으로 처리
                }

                Character_Data characterData = FindCharacterData(data.npcID);

                if (characterData != null)
                {
                    int levelCap = GetCurrentLevelCap(status);

                    status.level = Mathf.Clamp(status.level, 1, levelCap);

                    RecalculateStatsByLevel(status, characterData);
                }
            }
            else
            {
                // (선택사항) 만약 처음 보는 NPC라면 새로 생성해서 넣기
                // 하지만 보통 NPC 기본 데이터는 게임 시작시 초기화되므로 갱신만 하면 됨
            }
        }

        Debug.Log($"NPC {savedList.Count}명 데이터 복구 완료");
    }

    //불러오기 함수
    private Character_Data FindCharacterData(string characterID)
    {
        if (allcharacterDataList == null) return null;

        foreach (Character_Data data in allcharacterDataList)
        {
            if (data == null) continue;

            if (data.characterID == characterID) return data;
        }

        return null;
    }
    #endregion

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

    

    
}