using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatusBar : MonoBehaviour
{
    [SerializeField] private Slider HpSlider;
    [SerializeField] private Slider MpSlider;
    [SerializeField] private Slider ExpSlider;

    private Coroutine HpChangeRoutine;
    private Coroutine MpChangeRoutine;
    private Coroutine ExpChangeRoutine;

    public void UpdateStatus(Player_Stat stat)
    {
        // HP는 코루틴으로 부드럽게 변화
        if (HpChangeRoutine != null)
            StopCoroutine(HpChangeRoutine);
        HpChangeRoutine = StartCoroutine(AnimateSlider(HpSlider, (float)stat.CurrentHP / stat.MaxHP));

        // MP도 부드럽게 변화시키고 싶으면 아래처럼
        if (MpChangeRoutine != null)
            StopCoroutine(MpChangeRoutine);
        MpChangeRoutine = StartCoroutine(AnimateSlider(MpSlider, (float)stat.CurrentMP / stat.MaxMP));

        if (ExpChangeRoutine != null)
            StopCoroutine(ExpChangeRoutine);
        ExpChangeRoutine = StartCoroutine(AnimateSlider(ExpSlider, (float)stat.Exp / stat.LevelUpExp));
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
