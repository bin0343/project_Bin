using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("로딩 화면")]
    [SerializeField] private CanvasGroup loadingCanvasGroup;

    [Header("페이드")]
    [SerializeField, Min(0.01f)]
    private float fadeDuration = 0.3f;

    private bool isTransitioning;

    public bool IsTransitioning
    {
        get { return isTransitioning; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        } 

        Instance = this;

        SetLoadingScreenImmediate(false);
    }

    public void LoadScene(string sceneName, string targetSpawnID)
    {
        if (isTransitioning) return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneTransition] 씬 이름이 비어 있습니다.");
            return;
        }

        StartCoroutine(
            LoadSceneRoutine(sceneName, targetSpawnID)
        );
    }

    private IEnumerator LoadSceneRoutine(string sceneName, string targetSpawnID)
    {
        isTransitioning = true;

        CleanupSceneSpecificUI();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        BattleManager battleMgr = BattleManager.instance;
        GameObject activeCharacter = null;
        Player_Action activePlayerAction = null;

        if (battleMgr != null)
        {
            battleMgr.PreparePartyForSceneTransition();

            activeCharacter = battleMgr.GetActiveCharacter();

            if (activeCharacter != null)
            {
                activePlayerAction =
                    activeCharacter.GetComponent<Player_Action>();

                if (activePlayerAction != null)
                {
                    activePlayerAction.enabled = false;
                }
            }

            // 로딩 중 캐릭터 교대 입력 방지
            battleMgr.enabled = false;
        }

        yield return FadeLoadingScreen(1f);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        if (loadOperation == null)
        {
            Debug.LogError($"[SceneTransition] 씬 로딩 실패: {sceneName}");

            RestoreInput(battleMgr, activePlayerAction);
            isTransitioning = false;
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // 새 씬의 Awake가 처리될 시간을 확보
        yield return null;

        SceneSpawnPoint spawnPoint = FindSpawnPoint(targetSpawnID);

        if (spawnPoint != null && battleMgr != null)
        {
            battleMgr.MovePartyToSpawnPoint(spawnPoint.transform);
        }

        // 플레이어 배치가 끝난 뒤 보스 생성
        BossArenaManager bossArena = FindFirstObjectByType<BossArenaManager>();

        if (bossArena != null)
        {
            bossArena.InitializeArena();
        }

        RestoreInput(battleMgr, activePlayerAction);

        yield return FadeLoadingScreen(0f);

        isTransitioning = false;
    }

    private SceneSpawnPoint FindSpawnPoint(string targetSpawnID)
    {
        SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsSortMode.None);

        foreach (SceneSpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            if (spawnPoint.SpawnID == targetSpawnID)
            {
                return spawnPoint;
            }
        }

        Debug.LogError($"[SceneTransition] Spawn ID를 찾지 못했습니다: " + $"{targetSpawnID}");

        return null;
    }

    private void RestoreInput(
        BattleManager battleManager,
        Player_Action playerAction)
    {
        if (playerAction != null)
        {
            playerAction.enabled = true;
        }

        if (battleManager != null)
        {
            battleManager.enabled = true;
        }
    }

    private IEnumerator FadeLoadingScreen(float targetAlpha)
    {
        if (loadingCanvasGroup == null)
        {
            yield break;
        }

        loadingCanvasGroup.gameObject.SetActive(true);
        loadingCanvasGroup.blocksRaycasts = true;

        float startAlpha = loadingCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);

            loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);

            yield return null;
        }

        loadingCanvasGroup.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            loadingCanvasGroup.blocksRaycasts = false;
            loadingCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void SetLoadingScreenImmediate(bool visible)
    {
        if (loadingCanvasGroup == null) return;

        loadingCanvasGroup.alpha = visible ? 1f : 0f;
        loadingCanvasGroup.blocksRaycasts = visible;
        loadingCanvasGroup.gameObject.SetActive(visible);
    }

    private void CleanupSceneSpecificUI()
    {
        if (BossHpBar.instance != null)
        {
            BossHpBar.instance.Hide();
        }

        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.HideInteractionPrompt();
            UI_Manager.instance.SetBattleMode(false);
        }
    }
}
