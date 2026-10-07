using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    [Header("월드 위치 저장")]
    [SerializeField] private string persistentWorldSceneName = "City";
    [SerializeField, Min(1f)]
    private float worldPositionCheckInterval = 5f;
    [SerializeField, Min(0.1f)]
    private float worldPositionSaveDistance = 2f;

    private float worldPositionCheckTimer;

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
        if (!sessionReady || isApplyingSaveData) return;

        worldPositionCheckTimer += Time.unscaledDeltaTime;

        if (worldPositionCheckTimer >= worldPositionCheckInterval)
        {
            worldPositionCheckTimer = 0f;

            CheckWorldPositionChanged();
        }

        if (!saveDirty) return;

        autoSaveTimer -= Time.unscaledDeltaTime;

        if (autoSaveTimer <= 0f)
        {
            ForceSave();
        }
    }

    //자동 저장 함수
    public void RequestAutoSave()
    {
        if (!sessionReady || isApplyingSaveData) return;
        if (saveDirty) return;

        saveDirty = true;
        autoSaveTimer = autoSaveDelay;
    }

    private void CheckWorldPositionChanged()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName != persistentWorldSceneName) return;

        if (BattleManager.instance == null) return;

        if (!BattleManager.instance.TryGetActiveWorldPose(out Vector3 currentPosition, out float rotationY)) return;

        if (CurrentSaveData.world == null)
        {
            RequestAutoSave();
            return;
        }

        if (!CurrentSaveData.world.hasSavedPosition)
        {
            RequestAutoSave();
            return;
        }

        float sqrDistance = (currentPosition - CurrentSaveData.world.playerPosition).sqrMagnitude;
        float saveDistanceSqr = worldPositionSaveDistance * worldPositionSaveDistance;

        if (sqrDistance >= saveDistanceSqr)
        {
            RequestAutoSave();
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

    public bool IsTeleportActivated(string teleportID)
    {
        if (string.IsNullOrWhiteSpace(teleportID)) return false;

        if (CurrentSaveData?.world?.activatedTeleportIDs == null) return false;

        return CurrentSaveData.world.activatedTeleportIDs.Contains(teleportID);
    }

    public void MarkTeleportActivated(string teleportID)
    {
        if (string.IsNullOrWhiteSpace(teleportID))
        {
            Debug.LogWarning("[Save] Teleport ID가 비어 있습니다.");

            return;
        }

        if (CurrentSaveData.world == null)
        {
            CurrentSaveData.world = new WorldSaveData();
        }

        if (CurrentSaveData.world.activatedTeleportIDs == null)
        {
            CurrentSaveData.world.activatedTeleportIDs = new List<string>();
        }

        if (CurrentSaveData.world.activatedTeleportIDs.Contains(teleportID)) return;

        CurrentSaveData.world.activatedTeleportIDs.Add(teleportID);

        RequestAutoSave();

        Debug.Log($"[Save] 텔레포트 활성화 저장: " + $"{teleportID}");
    }

    public void SetLastTeleportPoint(string teleportID)
    {
        if (string.IsNullOrWhiteSpace(teleportID))
        {
            Debug.LogWarning("[Save] Last Teleport ID가 비어 있습니다.");
            return;
        }

        if (CurrentSaveData.world == null)
        {
            CurrentSaveData.world = new WorldSaveData();
        }

        if (CurrentSaveData.world.lastTeleportPointID == teleportID) return;

        CurrentSaveData.world.lastTeleportPointID = teleportID;

        RequestAutoSave();

        Debug.Log($"[Save] 마지막 안전 텔레포트 포인트 갱신: {teleportID}");
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
        if (QuestManager.instance != null)
        {
            CurrentSaveData.quests = QuestManager.instance.GetQuestSaveData();
        }

        GatherWorldData();
        GatherCustomMapPinData();
    }

    private void EnsureSaveContainers()
    {
        if (CurrentSaveData.account == null) CurrentSaveData.account = new AccountSaveData();
        if (CurrentSaveData.characters == null) CurrentSaveData.characters = new List<CharacterSaveData>();
        if (CurrentSaveData.party == null) CurrentSaveData.party = new PartySaveData();
        if (CurrentSaveData.inventory == null) CurrentSaveData.inventory = new InventorySaveData();
        if (CurrentSaveData.world == null) CurrentSaveData.world = new WorldSaveData();
        if (CurrentSaveData.world.activatedTeleportIDs == null) CurrentSaveData.world.activatedTeleportIDs = new List<string>();
        if (CurrentSaveData.world.clearedBossIDs == null) CurrentSaveData.world.clearedBossIDs = new List<string>();
        if (CurrentSaveData.world.customMapPins == null) CurrentSaveData.world.customMapPins = new List<CustomMapPinSaveData>();
        if (CurrentSaveData.quests == null) CurrentSaveData.quests = new List<QuestSaveData>();
    }

    private void GatherWorldData()
    {
        if (CurrentSaveData.world == null)
        {
            CurrentSaveData.world = new WorldSaveData();
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        // 보스맵 같은 인스턴스 전투씬에서는
        // 마지막 안전 월드 위치를 덮어쓰지 않는다.
        if (currentSceneName != persistentWorldSceneName) return;

        if (BattleManager.instance == null) return;

        if (!BattleManager.instance.TryGetActiveWorldPose(out Vector3 position, out float rotationY)) return;

        CurrentSaveData.world.sceneName = currentSceneName;
        CurrentSaveData.world.playerPosition = position;
        CurrentSaveData.world.playerRotationY = rotationY;
        CurrentSaveData.world.hasSavedPosition = true;
    }

    private void GatherCustomMapPinData()
    {
        if (SceneManager.GetActiveScene().name != persistentWorldSceneName) return;

        if (CurrentSaveData.world == null)
        {
            CurrentSaveData.world = new WorldSaveData();
        }

        MapPinManager pinManager = MapPinManager.instance;

        if (pinManager == null)
        {
            pinManager = FindObjectOfType<MapPinManager>(true);
        }

        if (pinManager == null) return;

        CurrentSaveData.world.customMapPins = pinManager.GetSaveData();
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
        if (QuestManager.instance != null)
        {
            QuestManager.instance.LoadQuestSaveData(CurrentSaveData.quests);
        }

        RestoreCustomMapPinsForCurrentScene();
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

        if (Character_Manager.Instance != null)
        {
            Character_Manager.Instance.InitializeNewParty();
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

    #region 복원
    public void RestoreWorldStateForCurrentScene()
    {
        if (CurrentSaveData == null || CurrentSaveData.world == null) return;

        WorldSaveData world = CurrentSaveData.world;

        if (!world.hasSavedPosition)
        {
            Debug.Log("[GameDataManager] 정확한 저장 위치가 없습니다. " + "마지막 텔레포트 포인트 복귀를 시도합니다.");

            RestoreToSafeFallback();

            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (world.sceneName != currentSceneName)
        {
            Debug.Log($"[GameDataManager] 저장된 씬 [{world.sceneName}]과 " + $"현재 씬 [{currentSceneName}]이 다릅니다. " + "마지막 텔레포트 포인트 복귀를 시도합니다.");

            RestoreToSafeFallback();

            return;
        }

        if (BattleManager.instance == null) return;

        BattleManager.instance.RestoreSavedWorldPose(world.playerPosition, world.playerRotationY);
    }

    private void RestoreToSafeFallback()
    {
        if (TryRestoreAtLastTeleportPoint()) return;

        TryRestoreAtDefaultStartPoint();
    }

    public void RestoreTeleportStatesForCurrentScene()
    {
        TeleportPoint3D[] teleportPoints = FindObjectsOfType<TeleportPoint3D>(true);

        foreach (TeleportPoint3D point in teleportPoints)
        {
            if (point == null) continue;

            string id = point.TeleportID;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"[Save] TeleportPoint3D에 " + $"Teleport ID가 없습니다. " + $"Object: {point.name}");

                continue;
            }

            bool activated = IsTeleportActivated(id);

            point.ApplySavedActivation(activated);
        }

        Debug.Log($"[Save] 텔레포트 상태 복원 완료 / " + $"{teleportPoints.Length}개");
    }

    private bool TryRestoreAtDefaultStartPoint()
    {
        if (BattleManager.instance == null) return false;

        Transform startPoint = BattleManager.instance.startSpawnPoint;

        if (startPoint == null)
        {
            Debug.LogError("[GameDataManager] BattleManager의 StartSpawnPoint가 없습니다.");

            return false;
        }

        BattleManager.instance.MovePartyToSpawnPoint(startPoint);

        Debug.Log("[GameDataManager] 기본 StartSpawnPoint로 복귀했습니다.");

        return true;
    }

    public void RestoreCustomMapPinsForCurrentScene()
    {
        if (CurrentSaveData?.world?.customMapPins == null) return;

        if (SceneManager.GetActiveScene().name != persistentWorldSceneName) return;

        MapPinManager pinManager = MapPinManager.instance;

        if (pinManager == null)
        {
            pinManager = FindObjectOfType<MapPinManager>(true);
        }

        if (pinManager == null) return;

        pinManager.LoadSaveData(CurrentSaveData.world.customMapPins);
    }

    #endregion


    public bool IsBossCleared(string bossID)
    {
        if (string.IsNullOrWhiteSpace(bossID)) return false;

        if (CurrentSaveData?.world?.clearedBossIDs == null) return false;

        return CurrentSaveData.world.clearedBossIDs.Contains(bossID);
    }

    public void MarkBossCleared(string bossID)
    {
        if (string.IsNullOrWhiteSpace(bossID)) return;

        if (CurrentSaveData.world == null)
        {
            CurrentSaveData.world =
                new WorldSaveData();
        }

        if (CurrentSaveData.world.clearedBossIDs == null)
        {
            CurrentSaveData.world.clearedBossIDs = new List<string>();
        }

        if (CurrentSaveData.world.clearedBossIDs.Contains(bossID)) return;

        CurrentSaveData.world.clearedBossIDs.Add(bossID);

        RequestAutoSave();

        Debug.Log($"[Save] 보스 최초 클리어 저장: " + $"{bossID}");
    }

    public void ResetBossClear(string bossID)
    {
        if (CurrentSaveData?.world?.clearedBossIDs == null)return;

        if (CurrentSaveData.world.clearedBossIDs.Remove(bossID))
        {
            RequestAutoSave();
        }
    }

    #region Helper

    private bool TryRestoreAtLastTeleportPoint()
    {
        if (CurrentSaveData?.world == null) return false;

        string lastTeleportID = CurrentSaveData.world.lastTeleportPointID;

        if (string.IsNullOrWhiteSpace(lastTeleportID)) return false;

        // 저장상 실제 활성화한 포인트인지도 확인
        if (!IsTeleportActivated(lastTeleportID))
        {
            Debug.LogWarning($"[GameDataManager] 마지막 텔레포트 ID가 " + $"활성화 목록에 없습니다: {lastTeleportID}");

            return false;
        }

        TeleportPoint3D[] teleportPoints = FindObjectsOfType<TeleportPoint3D>(true);

        foreach (TeleportPoint3D point in teleportPoints)
        {
            if (point == null) continue;
            if (point.TeleportID != lastTeleportID) continue;
            if (BattleManager.instance == null) return false;

            BattleManager.instance.MovePartyToSpawnPoint(point.transform);

            Debug.Log($"[GameDataManager] 마지막 텔레포트 포인트로 복귀: " + $"{lastTeleportID}");

            return true;
        }

        Debug.LogWarning($"[GameDataManager] 저장된 마지막 텔레포트 포인트를 " + $"현재 씬에서 찾지 못했습니다: {lastTeleportID}");

        return false;
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