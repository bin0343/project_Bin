using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FarmingStageManager : MonoBehaviour
{
    [Header("몬스터")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("웨이브")]
    [SerializeField, Min(1)]
    private int totalWaves = 2;
    [SerializeField, Min(1)]
    private int enemiesPerWave = 3;
    [SerializeField, Min(0f)]
    private float nextWaveDelay = 2f;

    [Header("클리어")]
    [SerializeField] private GameObject rewardChest;

    [Header("파밍맵 퇴장 설정")]
    [SerializeField] private string exitSceneName;
    [SerializeField] private string exitSpawnPointId;

    [Header("재도전")]
    [SerializeField] private string retrySpawnPointId = "FarmingEntry";

    private int currentWave;

    private readonly List<EnemyBase> currentWaveEnemies = new List<EnemyBase>();

    private bool isInitialized;

    private void Start()
    {
        if (SceneTransitionManager.instance == null || !SceneTransitionManager.instance.IsTransitioning)
        {
            InitializeStage();
        }
    }

    private void SpawnWave()
    {
        currentWaveEnemies.Clear();

        if (enemyPrefab == null)
        {
            Debug.LogError("[FarmingStage] Enemy Prefab이 없습니다.");

            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[FarmingStage] Spawn Point가 없습니다.");

            return;
        }

        for (int i = 0; i < enemiesPerWave; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];

            GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();

            if (enemy == null)
            {
                enemy = enemyObject.GetComponentInChildren<EnemyBase>();
            }

            if (enemy != null)
            {
                currentWaveEnemies.Add(enemy);
            }
            else
            {
                Debug.LogError($"[FarmingStage] {enemyObject.name}에서 " + "EnemyBase를 찾지 못했습니다.");
            }
        }
    }

    private bool AreCurrentWaveEnemiesDead()
    {
        if (currentWaveEnemies.Count == 0)  return false;

        foreach (EnemyBase enemy in currentWaveEnemies)
        {
            if (enemy != null && !enemy.isDead) return false;
        }

        return true;
    }

    private void ClearStage()
    {
        Debug.Log("[FarmingStage] 모든 웨이브 클리어!");

        if (rewardChest != null)
        {
            rewardChest.SetActive(true);
        }
    }

    public void InitializeStage()
    {
        if (isInitialized) return;

        isInitialized = true;

        RegisterExitUI();

        if (rewardChest != null)
        {
            rewardChest.SetActive(false);
        }

        StartCoroutine(StageRoutine());
    }

    public void ExitStage()
    {
        if (SceneTransitionManager.instance == null)
        {
            Debug.LogError("[FarmingStage] SceneTransitionManager가 없습니다.");

            return;
        }

        SceneTransitionManager.instance.LoadScene(exitSceneName, exitSpawnPointId);
    }

    public void RetryStage()
    {
        if (SceneTransitionManager.instance == null)
        {
            Debug.LogError("[FarmingStage] SceneTransitionManager가 없습니다.");

            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        SceneTransitionManager.instance.LoadScene(currentSceneName, retrySpawnPointId);
    }

    private void RegisterExitUI()
    {
        BossExitUI exitUI = FindObjectOfType<BossExitUI>(true);

        if (exitUI == null)
        {
            Debug.LogError("[FarmingStage] BossExitUI를 찾지 못했습니다.");

            return;
        }

        if (string.IsNullOrEmpty(exitSceneName) || string.IsNullOrEmpty(exitSpawnPointId))
        {
            Debug.LogError("[FarmingStage] 퇴장 씬 또는 Spawn ID가 비어 있습니다.");

            return;
        }

        exitUI.RegisterExitDestination(exitSceneName, exitSpawnPointId);
    }

    private IEnumerator StageRoutine()
    {
        for (int wave = 1; wave <= totalWaves; wave++)
        {
            currentWave = wave;

            Debug.Log($"[FarmingStage] Wave {currentWave} 시작");

            SpawnWave();

            yield return new WaitUntil(AreCurrentWaveEnemiesDead);

            Debug.Log($"[FarmingStage] Wave {currentWave} 클리어");

            if (wave < totalWaves)
            {
                yield return new WaitForSeconds(nextWaveDelay);
            }
        }

        ClearStage();
    }

    private void OnDestroy()
    {
        BossExitUI exitUI = FindObjectOfType<BossExitUI>(true);

        if (exitUI != null)
        {
            exitUI.ClearExitDestination();
        }
    }
}