using UnityEngine;
using Unity.Services.Authentication;

[DefaultExecutionOrder(-1000)]
public class GameSessionBootstrapper : MonoBehaviour
{
    [Header("게임 시작 로딩")]
    [SerializeField] private CityLoadingScreen loadingScreen;

    private async void Start()
    {
        loadingScreen?.Show();
        loadingScreen?.SetMessage("게임 데이터를 준비하는 중...");
        loadingScreen?.SetProgress(0);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("[Bootstrap] 로그인 정보가 없습니다.");

            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[Bootstrap] GameDataManager가 없습니다.");

            return;
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        GameDataManager.Instance.InitializeForPlayer(playerId);

        loadingScreen?.SetMessage("저장 정보를 확인하는 중...");
        loadingScreen?.SetProgress(10);

        CloudSaveManager cloudSaveManager = FindObjectOfType<CloudSaveManager>();

        if (cloudSaveManager == null)
        {
            Debug.LogError("[Bootstrap] CloudSaveManager가 없습니다.");
            return;
        }

        bool cloudInitialized = await cloudSaveManager.InitializeWriteLockAsync();

        if (!cloudInitialized)
        {
            Debug.LogError("[Bootstrap] 클라우드 상태 확인에 실패했습니다.");
            return;
        }

        loadingScreen?.SetMessage("클라우드 저장 정보를 확인하는 중...");
        loadingScreen?.SetProgress(25);

        StartupSaveResult saveResult = await cloudSaveManager.ResolveStartupSaveAsync();

        if (saveResult == StartupSaveResult.Failed)
        {
            Debug.LogError("[Bootstrap] 저장 데이터 동기화에 실패했습니다. " + "데이터 보호를 위해 게임 초기화를 중단합니다.");

            return;
        }

        loadingScreen?.SetMessage("저장 데이터를 적용하는 중...");
        loadingScreen?.SetProgress(45);

        bool loaded = false;

        if (saveResult == StartupSaveResult.NoSave)
        {
            GameDataManager.Instance.InitializeNewGame();
        }
        else
        {
            loaded = GameDataManager.Instance.LoadGame();

            if (!loaded)
            {
                Debug.LogError("[Bootstrap] 기존 저장 파일 로드에 실패했습니다. " + "신규 데이터로 덮어쓰지 않습니다.");

                return;
            }
        }

        loadingScreen?.SetMessage("캐릭터 정보를 불러오는 중...");
        loadingScreen?.SetProgress(60);

        if (loaded)
        {
            GameDataManager.Instance.RestoreTeleportStatesForCurrentScene();

            if (MiniMapTeleportManager.instance != null)
            {
                MiniMapTeleportManager.instance.RefreshMiniMapTeleports();
            }

            if (QuestManager.instance != null)
            {
                QuestManager.instance.RefreshSceneStateAfterLoad();
            }
        }

        loadingScreen?.SetMessage("월드 정보를 복구하는 중...");
        loadingScreen?.SetProgress(70);

        if (BattleManager.instance != null)
        {
            BattleManager.instance.InitializeParty();

            BattleManager.instance.SetPlayerControlLocked(true);

            loadingScreen?.SetMessage("파티를 배치하는 중...");
            loadingScreen?.SetProgress(82);

            if (loaded)
            {
                BattleManager.instance.RestoreActiveCharacterIndex(GameDataManager.Instance.SavedActiveCharacterIndex);

                GameDataManager.Instance.RestoreWorldStateForCurrentScene();
            }

            loadingScreen?.SetMessage("게임 시작을 준비하는 중...");
            loadingScreen?.SetProgress(92);
        }

        GameDataManager.Instance.MarkSessionReady();

        if (loaded)
        {
            // AP 오프라인 회복 등
            // Load 과정에서 바뀐 데이터를 다시 저장
            GameDataManager.Instance.RequestAutoSave();
        }
        else
        {
            // 신규 유저의 최초 세이브 생성
            GameDataManager.Instance.ForceSave();
        }

        if (loadingScreen != null)
        {
            await loadingScreen.CompleteAsync();
        }

        if (BattleManager.instance != null)
        {
            BattleManager.instance.SetPlayerControlLocked(false);
        }

        Debug.Log(loaded ? "[Bootstrap] 기존 세이브로 게임 시작": "[Bootstrap] 신규 계정으로 게임 시작");
    }
}