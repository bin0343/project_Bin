using UnityEngine;
using DG.Tweening;

public class PortalRevealEffect : MonoBehaviour
{
    [Header("포탈 비주얼")]
    [SerializeField] private Transform visualRoot;

    [Header("등장 연출")]
    [SerializeField]
    [Range(0.001f, 0.5f)]
    private float startScaleMultiplier = 0.03f;
    [SerializeField] private float openDuration = 0.25f;
    [SerializeField] private Ease openEase = Ease.OutBack;

    [Header("소멸 연출")]
    [SerializeField] private float closeDuration = 0.5f;
    [SerializeField] private Ease closeEase = Ease.InCubic;

    private Vector3 originalScale;
    private bool initialized;

    private void Initialize()
    {
        if (initialized) return;

        if (visualRoot == null)
        {
            Debug.LogWarning($"[PortalRevealEffect] {name}의 VisualRoot가 없습니다.");
            return;
        }

        originalScale = visualRoot.localScale;
        initialized = true;
    }

    public void PlayOpen()
    {
        Initialize();

        if (!initialized) return;

        gameObject.SetActive(true);

        visualRoot.DOKill();

        visualRoot.localScale = originalScale * startScaleMultiplier;

        visualRoot.DOScale(originalScale, openDuration).SetEase(openEase);
    }

    public void SetOpenImmediate()
    {
        Initialize();

        if (!initialized) return;

        visualRoot.DOKill();
        visualRoot.localScale = originalScale;

        gameObject.SetActive(true);
    }

    public void PlayClose()
    {
        Initialize();

        if (!initialized) return;

        visualRoot.DOKill();

        visualRoot.DOScale(
                originalScale * startScaleMultiplier,
                closeDuration)
            .SetEase(closeEase)
            .OnComplete(() =>
            {
                visualRoot.localScale = originalScale;
                gameObject.SetActive(false);
            });
    }

    public void CloseImmediate()
    {
        if (visualRoot != null)
        {
            visualRoot.DOKill();
        }

        gameObject.SetActive(false);
    }
}