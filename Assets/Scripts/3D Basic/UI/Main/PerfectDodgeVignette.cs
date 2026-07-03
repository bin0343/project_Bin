using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PerfectDodgeVignette : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup vignetteCanvasGroup;
    [SerializeField] private Image vignetteImage;

    [Header("Color")]
    [SerializeField] private Color vignetteColor = new Color(0f, 0f, 0f, 0.55f);

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.04f;
    [SerializeField] private float holdTime = 0.12f;
    [SerializeField] private float fadeOutTime = 0.18f;

    private Coroutine vignetteCoroutine;

    private void Awake()
    {
        if (vignetteCanvasGroup == null)
        {
            vignetteCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (vignetteImage == null)
        {
            vignetteImage = GetComponent<Image>();
        }

        if (vignetteImage != null)
        {
            vignetteImage.color = vignetteColor;
            vignetteImage.raycastTarget = false;
        }

        if (vignetteCanvasGroup != null)
        {
            vignetteCanvasGroup.alpha = 0f;
            vignetteCanvasGroup.blocksRaycasts = false;
            vignetteCanvasGroup.interactable = false;
        }
    }

    public void Play()
    {
        if (vignetteCanvasGroup == null)
            return;

        if (vignetteCoroutine != null)
        {
            StopCoroutine(vignetteCoroutine);
        }

        vignetteCoroutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        yield return Fade(0f, 1f, fadeInTime);
        yield return new WaitForSecondsRealtime(holdTime);
        yield return Fade(1f, 0f, fadeOutTime);

        vignetteCanvasGroup.alpha = 0f;
        vignetteCoroutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            vignetteCanvasGroup.alpha = to;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            vignetteCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        vignetteCanvasGroup.alpha = to;
    }
}