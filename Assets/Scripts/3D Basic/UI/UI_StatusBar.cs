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

    public void UpdateStatus(Character_Stat stat)
    {
        if (!isActiveAndEnabled) return;

        if (stat == null) return;

        CharacterStatus cStatus = Character_Manager.Instance.GetCharacterStatus(stat.characterData.characterID);
        
        if (levelText != null) levelText.text = $"Lv.{cStatus.level}";

        float hpRatio = (float)stat.currentHP / stat.maxHP;
        hpbar.DOFillAmount(hpRatio, 0.3f).SetUpdate(true);

        if (targetHP != stat.currentHP)
        {
            if (targetHP == -1) displayedHP = stat.currentHP;
            targetHP = stat.currentHP;

            DOTween.Kill(hpText);
            DOTween.To(() => displayedHP, x => {
                displayedHP = x;
                hpText.text = $"{displayedHP} / {stat.maxHP}";
            }, targetHP, 0.3f).SetTarget(hpText).SetUpdate(true);
        }
        else
        {
            // 타겟이 같더라도 maxHP가 변했을 수 있으므로 텍스트 유지
            hpText.text = $"{stat.currentHP} / {stat.maxHP}";
        }

        // 3. 경험치 바 및 텍스트
        float expRatio = (float)cStatus.currentExp / cStatus.maxExp;
        expbar.DOFillAmount(expRatio, 0.3f).SetUpdate(true);

        if (targetExp != cStatus.currentExp)
        {
            if (targetExp == -1) displayedExp = cStatus.currentExp;
            targetExp = cStatus.currentExp;

            DOTween.Kill(expText);
            DOTween.To(() => displayedExp, x => {
                displayedExp = x;
                expText.text = $"{displayedExp} / {cStatus.maxExp}";
            }, targetExp, 0.3f).SetTarget(expText).SetUpdate(true);
        }

        // 4. 스태미나 바 로직
        Account_Manager acc = Account_Manager.Instance;
        float staminaRatio = acc.currentStamina / acc.maxStamina;
        staminabar.fillAmount = staminaRatio;

        if (staminaRatio < 1f)
        {
            if (hideStaminaRoutine != null)
            {
                StopCoroutine(hideStaminaRoutine);
                hideStaminaRoutine = null;
            }

            if (staminaCanvasGroup.alpha < 1f)
            {
                staminaCanvasGroup.DOKill();
                staminaCanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
            }
        }
        else if (staminaRatio >= 1f && staminaCanvasGroup.alpha > 0f && hideStaminaRoutine == null)
        {
            hideStaminaRoutine = StartCoroutine(HideStamina(2f));
        }

        // 5. 탈진 효과 (빨간색 깜빡임)
        if (acc.isExhausted && !isFlashing)
        {
            isFlashing = true;
            staminaBackgroundImage.DOColor(Color.red, 0.2f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }
        else if (!acc.isExhausted && isFlashing)
        {
            isFlashing = false;
            staminaBackgroundImage.DOKill();
            staminaBackgroundImage.DOColor(Color.white, 0.2f).SetUpdate(true);
        }
    }

    private IEnumerator HideStamina(float delay)
    {
        yield return new WaitForSeconds(delay);

        staminaCanvasGroup.DOFade(0f, 0.5f);
        hideStaminaRoutine = null;
    }
}
