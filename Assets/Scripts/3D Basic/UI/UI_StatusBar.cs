using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatusBar : MonoBehaviour
{
    [SerializeField] private Image hpbar;
    [SerializeField] private Image mpbar;
    [SerializeField] private Image expbar;

    private Coroutine hpChangeRoutine;
    private Coroutine mpChangeRoutine;
    private Coroutine expChangeRoutine;

    public void UpdateStatus(Player_Stat stat)
    {
        // HP는 코루틴으로 부드럽게 변화
        if (hpChangeRoutine != null)
            StopCoroutine(hpChangeRoutine);
        hpChangeRoutine = StartCoroutine(AnimateSlider(hpbar, (float)stat.currentHP / stat.maxHP));

        // MP도 부드럽게 변화시키고 싶으면 아래처럼
        if (mpChangeRoutine != null)
            StopCoroutine(mpChangeRoutine);
        mpChangeRoutine = StartCoroutine(AnimateSlider(mpbar, (float)stat.currentMP / stat.maxMP));

        if (expChangeRoutine != null)
            StopCoroutine(expChangeRoutine);
        expChangeRoutine = StartCoroutine(AnimateSlider(expbar, (float)stat.exp / stat.levelUpExp));
    }

    private IEnumerator AnimateSlider(Image image, float targetValue)
    {
        float startValue = image.fillAmount;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.fillAmount = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }
        image.fillAmount = targetValue;
    }
}
