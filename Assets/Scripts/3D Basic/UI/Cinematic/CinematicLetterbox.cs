using UnityEngine;
using DG.Tweening;

public class CinematicLetterbox : MonoBehaviour
{
    public static CinematicLetterbox instance { get; private set; }

    [Header("검은 바")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;

    [Header("슬라이드 설정")]
    [SerializeField, Min(0.01f)]
    private float slideDuration = 0.4f;

    [SerializeField, Min(0f)]
    private float extraHideDistance = 10f;

    private Vector2 topShownPosition;
    private Vector2 bottomShownPosition;

    private Vector2 topHiddenPosition;
    private Vector2 bottomHiddenPosition;

    private Sequence currentSequence;
    private bool isInitialized;

    public float SlideDuration => slideDuration;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        Initialize();
        HideImmediate();
    }

    private void Initialize()
    {
        if (topBar == null || bottomBar == null)
        {
            Debug.LogError("[CinematicLetterbox] TopBar 또는 BottomBar가 없습니다.");
            return;
        }

        Canvas.ForceUpdateCanvases();

        topShownPosition = topBar.anchoredPosition;
        bottomShownPosition = bottomBar.anchoredPosition;

        topHiddenPosition = topShownPosition + Vector2.up * (topBar.rect.height + extraHideDistance);
        bottomHiddenPosition = bottomShownPosition + Vector2.down * (bottomBar.rect.height + extraHideDistance);

        isInitialized = true;
    }

    public void Show()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (!isInitialized) return;

        KillCurrentSequence();

        topBar.gameObject.SetActive(true);
        bottomBar.gameObject.SetActive(true);

        currentSequence = DOTween.Sequence();

        currentSequence.Append(topBar.DOAnchorPos(topShownPosition, slideDuration).SetEase(Ease.OutCubic)).Join(bottomBar.DOAnchorPos(bottomShownPosition, slideDuration).SetEase(Ease.OutCubic)).SetUpdate(UpdateType.Normal, true);
    }

    public void Hide()
    {
        if (!isInitialized) return;

        KillCurrentSequence();

        currentSequence = DOTween.Sequence();

        currentSequence.Append(topBar.DOAnchorPos(topHiddenPosition, slideDuration).SetEase(Ease.InCubic)).Join(bottomBar.DOAnchorPos(bottomHiddenPosition, slideDuration).SetEase(Ease.InCubic)).SetUpdate(UpdateType.Normal, true);
    }

    public void HideImmediate()
    {
        if (!isInitialized) return;

        KillCurrentSequence();

        topBar.anchoredPosition = topHiddenPosition;
        bottomBar.anchoredPosition = bottomHiddenPosition;

        topBar.gameObject.SetActive(true);
        bottomBar.gameObject.SetActive(true);
    }

    private void KillCurrentSequence()
    {
        if (currentSequence == null) return;

        currentSequence.Kill();
        currentSequence = null;
    }

    private void OnDestroy()
    {
        KillCurrentSequence();

        if (instance == this) instance = this;
    }
}
