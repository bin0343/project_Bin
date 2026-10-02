using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public GameSaveData CurrentSaveData { get; private set; } = new GameSaveData();

    private string saveFilePath;

    [Header("자동 저장")]
    [SerializeField, Min(0.5f)]
    private float autoSaveDelay = 2f;

    private bool sessionReady;
    private bool saveDirty;
    private float autoSaveTimer;
    private bool isApplyingSaveData;

    [Header("게임의 모든 아이템")]
    public List<Item_Base> allGameItems; // 인스펙터에서 드래그해서 넣기
    private Dictionary<string, Item_Base> itemDB = new Dictionary<string, Item_Base>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeItemDB();
    }

    //빠른저장
    private void Update()
    {
        if (!sessionReady || !saveDirty || isApplyingSaveData) return;

        autoSaveTimer -= Time.unscaledDeltaTime;

        if (autoSaveTimer <= 0f) ForceSave();
    }

    //자동 저장 함수
    public void RequestAutoSave()
    {
        if (!sessionReady || isApplyingSaveData) return;

        saveDirty = true;
        autoSaveTimer = autoSaveDelay;
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

    public void InitializeForPlayer(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogError("[GameDataManager] PlayerId가 비어 있습니다.");

            return;
        }

        string playerSaveFolder = Path.Combine(Application.persistentDataPath, "Saves", playerId);

        Directory.CreateDirectory(playerSaveFolder);

        saveFilePath = Path.Combine(playerSaveFolder, "GameSave_v1.json");

        sessionReady = false;
        saveDirty = false;

        Debug.Log($"[GameDataManager] 저장 대상 PlayerId: {playerId}\n" + $"[GameDataManager] 저장 경로: {saveFilePath}");
    }

    // ID로 아이템 원본 찾기
    public Item_Base GetItemByID(string id)
    {
        if (itemDB.ContainsKey(id)) return itemDB[id];
        return null;
    }

    // --- [저장 (Save)] ---
    public void ForceSave()
    {
        if (!sessionReady) return;

        if (string.IsNullOrEmpty(saveFilePath))
        {
            Debug.LogError("[GameDataManager] 저장 경로가 없습니다.");

            return;
        }

        try
        {
            GatherGameData();

            string json = JsonUtility.ToJson(CurrentSaveData, true);

            File.WriteAllText(saveFilePath, json);

            saveDirty = false;

            Debug.Log($"[GameDataManager] 자동 저장 완료: " + $"{saveFilePath}");
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[GameDataManager] 저장 실패\n" + $"{exception}");
        }
    }

    // --- [로드 (Load)] ---
    public bool LoadGame()
    {
        if (string.IsNullOrEmpty(saveFilePath))
        {
            Debug.LogError("[GameDataManager] 저장 경로가 없습니다.");

            return false;
        }

        if (!File.Exists(saveFilePath))
        {
            Debug.Log("[GameDataManager] 신규 계정입니다. " + "저장 파일이 없습니다.");

            return false;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);

            GameSaveData loadedData = JsonUtility.FromJson<GameSaveData>(json);

            if (loadedData == null) return false;

            CurrentSaveData = loadedData;

            EnsureSaveContainers();

            isApplyingSaveData = true;

            ApplyGameData();

            isApplyingSaveData = false;

            Debug.Log("[GameDataManager] 저장 데이터 로드 완료");

            return true;
        }
        catch (System.Exception exception)
        {
            isApplyingSaveData = false;

            Debug.LogError($"[GameDataManager] 로드 실패\n" + $"{exception}");

            return false;
        }
    }

    // --- [데이터 수집 (Game -> SaveData)] ---
    private void GatherGameData()
    {
        if (CurrentSaveData == null)
        {
            CurrentSaveData = new GameSaveData();
        }

        if (Account_Manager.Instance != null)
        {
            CurrentSaveData.account = Account_Manager.Instance.GetSaveData();
        }

        if (Character_Manager.Instance != null)
        {
            CurrentSaveData.characters = Character_Manager.Instance.GetCharacterSaveData();

            CurrentSaveData.party = Character_Manager.Instance.GetPartySaveData();
        }

        if (BattleManager.instance != null && CurrentSaveData.party != null)
        {
            CurrentSaveData.party.activeCharacterIndex = BattleManager.instance.CurrentActiveIndex;
        }

        if (Player_Inventory.instance != null)
        {
            CurrentSaveData.inventory = Player_Inventory.instance.GetSaveData();
        }
    }

    private void EnsureSaveContainers()
    {
        if (CurrentSaveData.account == null) CurrentSaveData.account = new AccountSaveData();

        if (CurrentSaveData.characters == null) CurrentSaveData.characters = new List<CharacterSaveData>();

        if (CurrentSaveData.party == null) CurrentSaveData.party = new PartySaveData();

        if (CurrentSaveData.inventory == null) CurrentSaveData.inventory = new InventorySaveData();

        if (CurrentSaveData.world == null) CurrentSaveData.world = new WorldSaveData();

        if (CurrentSaveData.quests == null) CurrentSaveData.quests = new List<QuestSaveData>();
    }

    // --- [데이터 적용 (SaveData -> Game)] ---
    private void ApplyGameData()
    {
        if (Account_Manager.Instance != null)
        {
            Account_Manager.Instance.LoadSaveData(CurrentSaveData.account);
        }

        if (Character_Manager.Instance != null)
        {
            Character_Manager.Instance.LoadCharacterSaveData(CurrentSaveData.characters);

            Character_Manager.Instance.LoadPartySaveData(CurrentSaveData.party);
        }

        if (Player_Inventory.instance != null)
        {
            Player_Inventory.instance.LoadSaveData(CurrentSaveData.inventory);
        }
    }

    #region 신규 초기화
    //신규 계정 초기화
    public void InitializeNewGame()
    {
        CurrentSaveData = new GameSaveData();

        if (Account_Manager.Instance != null)
        {
            Account_Manager.Instance.InitializeNewAccount();
        }

        if (Player_Inventory.instance != null)
        {
            Player_Inventory.instance.InitializeNewInventory();
        }

        Debug.Log("[GameDataManager] 신규 플레이어 초기화 완료");
    }

    public int SavedActiveCharacterIndex
    {
        get
        {
            if (CurrentSaveData?.party == null)
            {
                return 0;
            }

            return CurrentSaveData.party.activeCharacterIndex;
        }
    }

    public void MarkSessionReady()
    {
        sessionReady = true;
    }

    #endregion

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            ForceSave();
        }
    }

    private void OnApplicationQuit()
    {
        ForceSave();
    }
}