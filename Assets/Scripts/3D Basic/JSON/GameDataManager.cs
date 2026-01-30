using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;

    public SaveData saveData = new SaveData();
    private string saveFilePath;

    [Header("게임의 모든 아이템")]
    public List<Item_Base> allGameItems; // 인스펙터에서 드래그해서 넣기
    private Dictionary<string, Item_Base> itemDB = new Dictionary<string, Item_Base>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
        if (Player_Stat.globalInstance != null)
        {
            saveData.playerName = PlayerPrefs.GetString("PlayerName", "플레이어"); // 이름은 PlayerPrefs에서 가져오거나 별도 관리
            saveData.playerLevel = Player_Stat.globalInstance.level;
            saveData.playerGold = Player_Stat.globalInstance.gold;
            saveData.playerExp = Player_Stat.globalInstance.exp;

            if (Player_Stat.globalInstance.baseStats != null)
            {
                saveData.baseStats = (float[])Player_Stat.globalInstance.baseStats.Clone();
            }
        }

        // 2. 인벤토리 저장
        saveData.inventoryList.Clear();
        if (Player_Inventory.instance != null)
        {
            List<ItemHolder> slots = Player_Inventory.instance.inventorySlots;
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
        if (NPC_Manager.instance != null)
        {
            saveData.npcList = NPC_Manager.instance.GetSaveData();
        }
        // (나중에 장비, NPC, 퀘스트 등 추가)
    }

    // --- [데이터 적용 (SaveData -> Game)] ---
    void ApplyGameData()
    {
        // 1. 플레이어 스탯 복구
        if (Player_Stat.globalInstance != null)
        {
            Player_Stat.globalInstance.LoadStatsFromSaveData(
                saveData.playerLevel,
                saveData.playerGold,
                saveData.playerExp,
                saveData.baseStats
            );

            // UI 갱신 (로비 매니저가 있다면)
            if (LobbyManager.instance != null) LobbyManager.instance.RefreshUserInfo();
        }

        // 2. 인벤토리 복구
        if (Player_Inventory.instance != null)
        {
            // 기존 인벤토리 싹 비우기 (중복 방지)
            Player_Inventory.instance.inventorySlots.Clear();

            // 빈 슬롯들 미리 채우기 (기존 인벤토리 크기만큼, 예: 20칸)
            // Player_Inventory에 슬롯 초기화 로직이 없다면 여기서 임의로 채움
            // 보통 Start에서 초기화하므로, 여기서는 Clear 후 AddItem으로 넣거나 직접 할당

            // 여기서는 리스트를 새로 만드는 방식 사용
            // (주의: Player_Inventory 구조에 따라 다를 수 있음. 일단 AddItem 방식 추천)

            // [방식 A] 인벤토리 초기화 후 AddItem으로 하나씩 넣기
            // Player_Inventory의 구조상 inventorySlots가 고정 크기인지 가변 리스트인지 확인 필요.
            // 보내주신 코드를 보니 List<ItemHolder> inventorySlots = new List<ItemHolder>(); 네요.

            Player_Inventory.instance.inventorySlots.Clear();
            // 기본 슬롯 30개 생성 (빈 칸)
            for (int i = 0; i < 30; i++) Player_Inventory.instance.inventorySlots.Add(null);

            foreach (var savedItem in saveData.inventoryList)
            {
                Item_Base itemOriginal = GetItemByID(savedItem.itemID);
                if (itemOriginal != null)
                {
                    // 해당 위치에 아이템 복구
                    if (savedItem.slotIndex < Player_Inventory.instance.inventorySlots.Count)
                    {
                        Player_Inventory.instance.inventorySlots[savedItem.slotIndex]
                            = new ItemHolder(itemOriginal, savedItem.quantity);
                    }
                }
            }

            // 인벤토리 UI 갱신
            Player_Inventory.instance.RefreshAllUI();
        }

        //NPC로드
        if (NPC_Manager.instance != null)
        {
            NPC_Manager.instance.LoadFromSaveData(saveData.npcList);
        }
    }
}