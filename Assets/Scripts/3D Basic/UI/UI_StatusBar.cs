using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_StatusBar : MonoBehaviour
{
    [SerializeField] private Image hpbar;
    [SerializeField] private Image expbar;
    [SerializeField] private Image staminabar;
    [SerializeField] private Text hpText;
    [SerializeField] private Text expText;
    [SerializeField] private Text levelText;

    [SerializeField] private CanvasGroup staminaCanvasGroup;
    [SerializeField] private Image staminaBackgroundImage;

    private Coroutine hideStaminaRoutine;
    private bool isFlashing = false;    //깜빡임 연출 중복 방지

    private int targetHP = -1;
    private int displayedHP = 0;
    private int targetExp = -1;
    private int displayedExp = 0;

    public void UpdateStatus(Player_Stat stat)
    {
        levelText.text = $"Lv.{stat.level}";
        hpbar.DOFillAmount((float)stat.currentHP / stat.maxHP, 0.3f);
        expbar.DOFillAmount((float)stat.exp / stat.levelUpExp, 0.3f);
        staminabar.DOFillAmount(stat.currentStamina / stat.maxStamina, 0.3f);

        if (targetHP != stat.currentHP)
        {
            // 게임 시작 직후 초기화 처리
            if (targetHP == -1)
            {
                displayedHP = stat.currentHP;
                hpText.text = $"{displayedHP} / {stat.maxHP}";
            }

            targetHP = stat.currentHP;

            DOTween.Kill(hpText); // 기존에 진행 중이던 텍스트 애니메이션 중지

            DOTween.To(() => displayedHP, x =>
            {
                displayedHP = x;
                hpText.text = $"{displayedHP} / {stat.maxHP}";
            }, targetHP, 0.3f).SetTarget(hpText);
        }

        expbar.DOFillAmount((float)stat.exp / stat.levelUpExp, 0.3f);
        if (targetExp != stat.exp)
        {
            // 게임 시작 직후 초기화 처리
            if (targetExp == -1)
            {
                displayedExp = stat.exp;
                expText.text = $"{displayedExp} / {stat.levelUpExp}";
            }

            targetExp = stat.exp;
            DOTween.Kill(expText); // 기존에 진행 중이던 텍스트 애니메이션 중지

            DOTween.To(() => displayedExp, x =>
            {
                displayedExp = x;
                expText.text = $"{displayedExp} / {stat.levelUpExp}";
            }, targetExp, 0.3f).SetTarget(expText);
        }

        float staminaRatio = stat.currentStamina / stat.maxStamina;
        staminabar.DOFillAmount(staminaRatio, 0.3f);

        // 스태미나 바 표시/숨김
        if (staminaRatio < 1f)
        {
            // 스태미나가 소모 중일 때: 숨기기 타이머를 취소하고 즉시 나타남
            if (hideStaminaRoutine != null)
            {
                StopCoroutine(hideStaminaRoutine);
                hideStaminaRoutine = null;
            }

            // 알파값이 1이 아니라면 DOTween으로 0.2초 동안 부드럽게 나타나게 함
            if (staminaCanvasGroup.alpha < 1f)
            {
                staminaCanvasGroup.DOFade(1f, 0.2f);
            }
        }
        else if (staminaRatio >= 1f)
        {
            // 스태미나가 100% 꽉 차고, 화면에 보이고 있으며, 타이머가 안 돌고 있을 때만 타이머 시작
            if (hideStaminaRoutine == null && staminaCanvasGroup.alpha > 0f)
            {
                hideStaminaRoutine = StartCoroutine(HideStamina(2f));
            }
        }

        if (stat.isExhausted && !isFlashing)
        {
            isFlashing = true;
            staminaBackgroundImage.DOColor(Color.red, 0.2f).SetLoops(-1, LoopType.Yoyo);
        }
        else if (!stat.isExhausted && isFlashing)
        {
            isFlashing = false;
            staminaBackgroundImage.DOKill();
            staminaBackgroundImage.color = Color.white;
        }
    }

    private IEnumerator HideStamina(float delay)
    {
        yield return new WaitForSeconds(delay);

        staminaCanvasGroup.DOFade(0f, 0.5f);
        hideStaminaRoutine = null;
    }
}
