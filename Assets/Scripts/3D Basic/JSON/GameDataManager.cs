using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public SaveData saveData = new SaveData();
    public Player_Data playerData;
    private string saveFilePath;

    [Header("게임의 모든 아이템")]
    public List<Item_Base> allGameItems; // 인스펙터에서 드래그해서 넣기
    private Dictionary<string, Item_Base> itemDB = new Dictionary<string, Item_Base>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "MyGameSave.json");

            // 아이템 DB 구축 (리스트 -> 딕셔너리 변환)
            InitializeItemDB();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //빠른저장
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
            Debug.Log("F5키 입력: 빠른 저장 완료");
        }
    }

    // --- [아이템 DB 초기화] ---
    void InitializeItemDB()
    {
        itemDB.Clear();
        foreach (var item in allGameItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemID))
            {
                if (!itemDB.ContainsKey(item.itemID))
                {
                    itemDB.Add(item.itemID, item);
                }
                else
                {
                    Debug.LogWarning($"중복된 아이템 ID 발견: {item.itemID}");
                }
            }
        }
        Debug.Log($"아이템 DB 구축 완료: {itemDB.Count}개");
    }

    // ID로 아이템 원본 찾기
    public Item_Base GetItemByID(string id)
    {
        if (itemDB.ContainsKey(id)) return itemDB[id];
        return null;
    }

    // --- [저장 (Save)] ---
    public void SaveGame()
    {
        GatherGameData(); // 현재 상태 수집
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"게임 저장 완료: {saveFilePath}");
    }

    // --- [로드 (Load)] ---
    public bool LoadGame()
    {
        if (!File.Exists(saveFilePath)) return false;

        string json = File.ReadAllText(saveFilePath);
        saveData = JsonUtility.FromJson<SaveData>(json);

        ApplyGameData(); // 게임에 적용
        return true;
    }

    // --- [데이터 수집 (Game -> SaveData)] ---
    void GatherGameData()
    {
        // 1. 플레이어 스탯 저장
        if (Account_Manager.Instance != null)
        {
            saveData.playerName = PlayerPrefs.GetString("PlayerName", "플레이어"); // 이름은 PlayerPrefs에서 가져오거나 별도 관리
            saveData.playerLevel = Account_Manager.Instance.accountLevel;
            saveData.playerGold = Account_Manager.Instance.gold;
            saveData.playerExp = Account_Manager.Instance.accountExp;
        }

        // 2. 인벤토리 저장
        saveData.inventoryList.Clear();
        if (Player_Inventory.Instance != null)
        {
            List<ItemHolder> slots = Player_Inventory.Instance.inventorySlots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].ItemData != null)
                {
                    ItemSaveData itemData = new ItemSaveData();
                    itemData.itemID = slots[i].ItemData.itemID;
                    itemData.quantity = slots[i].Quantity;
                    itemData.slotIndex = i;
                    saveData.inventoryList.Add(itemData);
                }
            }
        }

        //NPC저장
        if (Character_Manager.Instance != null)
        {
            saveData.npcList = Character_Manager.Instance.GetSaveData();
        }

        //퀘스트 저장
        if (QuestManager.instance != null)
        {
            saveData.questList = QuestManager.instance.GetQuestSaveData();
        }
        // (나중에 장비, NPC, 퀘스트 등 추가)
    }

    // --- [데이터 적용 (SaveData -> Game)] ---
    void ApplyGameData()
    {
        if (playerData != null)
        {
            playerData.characterName = saveData.playerName;
        }

        // 1. 플레이어 스탯 복구
        if (Account_Manager.Instance != null)
        {
            Account_Manager.Instance.accountLevel = saveData.playerLevel;
            Account_Manager.Instance.gold = saveData.playerGold;
            Account_Manager.Instance.accountExp = saveData.playerExp;

            if (LobbyManager.Instance != null) LobbyManager.Instance.RefreshUserInfo();
        }

        // 2. 인벤토리 복구
        if (Player_Inventory.Instance != null)
        {
            // 기존 인벤토리 싹 비우기 (중복 방지)
            Player_Inventory.Instance.inventorySlots.Clear();

            Player_Inventory.Instance.inventorySlots.Clear();
            // 기본 슬롯 30개 생성 (빈 칸)
            for (int i = 0; i < 30; i++) Player_Inventory.Instance.inventorySlots.Add(null);

            foreach (var savedItem in saveData.inventoryList)
            {
                Item_Base itemOriginal = GetItemByID(savedItem.itemID);
                if (itemOriginal != null)
                {
                    // 해당 위치에 아이템 복구
                    if (savedItem.slotIndex < Player_Inventory.Instance.inventorySlots.Count)
                    {
                        Player_Inventory.Instance.inventorySlots[savedItem.slotIndex]
                            = new ItemHolder(itemOriginal, savedItem.quantity);
                    }
                }
            }

            // 인벤토리 UI 갱신
            Player_Inventory.Instance.RefreshAllUI();
        }

        //NPC로드
        if (Character_Manager.Instance != null)
        {
            Character_Manager.Instance.LoadFromSaveData(saveData.npcList);
        }
        //퀘스트 로드
        if (QuestManager.instance != null)
        {
            QuestManager.instance.LoadQuestSaveData(saveData.questList);
        }
    }
}