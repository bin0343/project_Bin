using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [Header("보스 생성")]
    [SerializeField] private BossEnemy bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("생성된 보스 확인")]
    [SerializeField] private BossEnemy currentBoss;

    private bool isInitialized;

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

        currentBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);

        Debug.Log($"[BossArena] 보스 생성 완료: " + $"{currentBoss.gameObject.name}");
    }
}