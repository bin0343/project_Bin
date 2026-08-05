using System.Collections;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class BossIntroController : MonoBehaviour
{
    public static BossIntroController Instance
    {
        get;
        private set;
    }

    [Header("소개 카메라")]
    [SerializeField] private CinemachineVirtualCamera introCamera;
    [SerializeField] private Transform cameraStartPoint;
    [SerializeField] private Transform cameraEndPoint;

    [Header("Cinemachine 우선순위")]
    [SerializeField] private int idlePriority = 0;
    [SerializeField] private int cinematicPriority = 100;

    [Header("연출 시간")]
    [SerializeField, Min(0f)]
    private float blendInWait = 0.6f;
    [SerializeField, Min(0.01f)]
    private float cameraMoveDuration = 2.5f;
    [SerializeField, Min(0f)]
    private float bossHoldDuration = 0.8f;
    [SerializeField, Min(0f)]
    private float blendOutWait = 0.6f;

    [SerializeField, Min(0f)]
    private float jumpBeforeBlendOutDelay = 0.25f;

    [Header("레터박스")]
    [SerializeField] private CinematicLetterbox letterbox;

    private Coroutine introCoroutine;
    private Tween cameraMoveTween;

    // 같은 보스맵에 머무르는 동안 한 번만 재생
    private bool hasPlayedThisArenaSession;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (introCamera != null)
        {
            introCamera.Priority = idlePriority;
            introCamera.gameObject.SetActive(false);
        }

        if (letterbox == null) letterbox = FindObjectOfType<CinematicLetterbox>(true);
    }

    private void Start()
    {
        if (letterbox == null) letterbox = CinematicLetterbox.Instance;
    }

    public bool TryPlayIntro(BossEnemy boss)
    {
        if (boss == null) return false;

        if (IsPlaying || hasPlayedThisArenaSession) return false;

        if (introCamera == null || cameraStartPoint == null || cameraEndPoint == null)
        {
            Debug.LogError("[BossIntro] 카메라 또는 시작/종료 지점이 없습니다.");

            return false;
        }

        hasPlayedThisArenaSession = true;

        introCoroutine = StartCoroutine(PlayIntroRoutine(boss));

        return true;
    }

    private IEnumerator PlayIntroRoutine(BossEnemy boss)
    {
        IsPlaying = true;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetPlayerControlLocked(true);
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.EnterCinematicMode();
        }

        if (letterbox != null) letterbox.Show();

        introCamera.gameObject.SetActive(true);

        introCamera.transform.SetPositionAndRotation(cameraStartPoint.position, cameraStartPoint.rotation);

        introCamera.Follow = null;
        introCamera.LookAt = boss.IntroCameraTarget;

        introCamera.Priority = cinematicPriority;

        yield return new WaitForSecondsRealtime(blendInWait);

        cameraMoveTween?.Kill();

        cameraMoveTween = introCamera.transform.DOMove(cameraEndPoint.position, cameraMoveDuration).SetEase(Ease.InOutSine).SetUpdate(UpdateType.Normal, true);

        yield return new WaitForSecondsRealtime(cameraMoveDuration);

        if (bossHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(bossHoldDuration);
        }

        introCamera.LookAt = null;

        bool openingStarted = boss.BeginOpeningLeap();

        if (openingStarted && jumpBeforeBlendOutDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(jumpBeforeBlendOutDelay);
        }

        introCamera.Priority = idlePriority;

        if (letterbox != null) letterbox.Hide();

        float finishWait = blendOutWait;

        if (letterbox != null)
        {
            finishWait = Mathf.Max(finishWait, letterbox.SlideDuration);
        }

        yield return new WaitForSecondsRealtime(finishWait);

        introCamera.gameObject.SetActive(false);

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.ExitCinematicMode();
        }


        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetPlayerControlLocked(false);
        }

        boss.ReleaseOpeningLeapTargeting();

        IsPlaying = false;
        introCoroutine = null;
    }

    private void OnDisable()
    {
        cameraMoveTween?.Kill();

        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
            introCoroutine = null;
        }

        if (introCamera != null)
        {
            introCamera.Priority = idlePriority;
            introCamera.gameObject.SetActive(false);
        }

        if (letterbox != null)
        {
            letterbox.HideImmediate();
        }

        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.ExitCinematicMode();
        }

        if (IsPlaying && BattleManager.Instance != null)
        {
            BattleManager.Instance.SetPlayerControlLocked(false);
        }

        IsPlaying = false;
    }

    private void OnDestroy()
    {
        cameraMoveTween?.Kill();

        if (Instance == this) Instance = null;
    }
}
