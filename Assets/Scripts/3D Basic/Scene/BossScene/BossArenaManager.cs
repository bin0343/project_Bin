using System.Collections;
using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    public static BossArenaManager Instance {  get; private set; }

    [Header("보스 생성")]
    [SerializeField] private BossEnemy bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("생성된 보스 확인")]
    [SerializeField] private BossEnemy currentBoss;

    [Header("보스 재생성")]
    [SerializeField, Min(0f)]
    private float respawnDelay = 5f;
    private Coroutine respawnCoroutine;
    private bool hasSpawnedOnce;

    [Header("보스맵 퇴장 설정")]
    [SerializeField]
    private string exitSceneName;

    [SerializeField]
    private string exitSpawnPointId;

    public string ExitSceneName
    {
        get { return exitSceneName; }
    }

    public string ExitSpawnPointId
    {
        get { return exitSpawnPointId; }
    }

    private bool isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // 보스맵 씬을 에디터에서 직접 실행했을 때를 위한 처리
        if (SceneTransitionManager.Instance == null || !SceneTransitionManager.Instance.IsTransitioning)
        {
            InitializeArena();
        }
    }

    public void InitializeArena()
    {
        if (isInitialized) return;

        isInitialized = true;

        RegisterExitUI();
        SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (currentBoss != null)
        {
            Debug.LogWarning("[BossArena] 이미 보스가 존재합니다.");

            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError("[BossArena] Boss Prefab이 설정되지 않았습니다.");

            return;
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogError("[BossArena] Boss Spawn Point가 없습니다.");

            return;
        }

        bool isRespawn = hasSpawnedOnce;

        currentBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        currentBoss.ConfigureSpawn(isRespawn);

        hasSpawnedOnce = true;

        Debug.Log($"[BossArena] 보스 생성 완료: " + $"{currentBoss.gameObject.name}");
    }

    private void RegisterExitUI()
    {
        BossExitUI exitUI = FindObjectOfType<BossExitUI>(true);

        if (exitUI == null) return;

        exitUI.RegisterExitDestination(exitSceneName, exitSpawnPointId);
    }

    public void NotifyRewardClaimed()
    {
        if (respawnCoroutine != null) return;

        respawnCoroutine = StartCoroutine(RespawnRoutine());

        UI_Manager.Instance.ShowMessage("잠시 후 보스가 재생성 됩니다.");
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log("[BossArena] 보상 수령 완료. 보스 재생성을 준비합니다.");

        if (currentBoss != null)
        {
            Destroy(currentBoss.gameObject);
            currentBoss = null;
        }

        if (respawnDelay > 0f)
        {
            yield return new WaitForSeconds(respawnDelay);
        }

        SpawnBoss();

        respawnCoroutine = null;
    }

    private void OnDestroy()
    {
        BossExitUI exitUI = FindObjectOfType<BossExitUI>(true);

        if (exitUI != null)
        {
            exitUI.ClearExitDestination();
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}