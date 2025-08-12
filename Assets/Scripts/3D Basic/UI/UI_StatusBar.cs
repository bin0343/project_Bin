using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatusBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider mpSlider;

    private Coroutine hpChangeRoutine;
    private Coroutine mpChangeRoutine;

    public void UpdateStatus(Player_Stat stat)
    {
        // HP는 코루틴으로 부드럽게 변화
        if (hpChangeRoutine != null)
            StopCoroutine(hpChangeRoutine);
        hpChangeRoutine = StartCoroutine(AnimateSlider(hpSlider, (float)stat.CurrentHP / stat.MaxHP));

        // MP도 부드럽게 변화시키고 싶으면 아래처럼
        if (mpChangeRoutine != null)
            StopCoroutine(mpChangeRoutine);
        mpChangeRoutine = StartCoroutine(AnimateSlider(mpSlider, (float)stat.CurrentMP / stat.MaxMP));
    }

    private IEnumerator AnimateSlider(Slider slider, float targetValue)
    {
        float startValue = slider.value;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }
        slider.value = targetValue;
    }
}
