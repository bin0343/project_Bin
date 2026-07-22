using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHpBar : MonoBehaviour
{
    public static BossHpBar instance {  get; private set; }

    [Header("보스 UI")]
    [SerializeField] private GameObject bossPanel;

    [Header("보스 정보")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text bossNameText;

    [Header("체력바")]
    [SerializeField] private Image hpbarFill;
    [Tooltip("깎인 체력을 잠시 표시하는 흰색 이미지")]
    [SerializeField] private Image damageBarFill;
    [Header("피해 체력 연출")]
    [Tooltip("흰색 피해 바가 줄어들기 전 유지되는 시간")]
    [SerializeField] private float damageHoldDuration = 0.25f;
    [Tooltip("흰색 피해 바가 실제 체력까지 줄어드는 시간")]
    [SerializeField] private float damageShrinkDuration = 0.4f;

    [Header("스턴 게이지 - 임시")]
    [SerializeField] private GameObject stunGaugeRoot;
    [SerializeField] private Image StunGaugeFill;

    private Enemy_Stat currentBossStat;
    private Coroutine damageBarCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (bossPanel != null) bossPanel.SetActive(false);

        if (stunGaugeRoot != null) stunGaugeRoot.SetActive(false);
    }

    public void Show(Enemy_Stat bossStat)
    {
        if(bossStat == null)
        {
            Debug.LogWarning("[Boss UI] 연결할 Enemy_Stat이 없습니다.");
            return;
        }

        UnbindCurrentBoss();

        currentBossStat = bossStat;

        currentBossStat.OnHpChanged += HandleHpChanged;
        currentBossStat.OnDied += Hide;

        if (bossNameText != null)
        {
            bossNameText.text = string.IsNullOrEmpty(currentBossStat.EnemyName) ? currentBossStat.gameObject.name : currentBossStat.EnemyName;
        }

        if (levelText != null)
        {
            levelText.text = $"Lv. {currentBossStat.EnemyLevel}";
        }

        SetHpImmediately(currentBossStat.currentHP, currentBossStat.maxHP);

        if (bossPanel != null) bossPanel.SetActive(true);
    }

    public void HandleHpChanged(int currentHp, int maxHp)
    {
        if (hpbarFill == null) return;

        float targetFillAmount = CalculateFillAmount(currentHp, maxHp);

        float previousFillAmount = hpbarFill.fillAmount;

        bool tookDamage = targetFillAmount < previousFillAmount;

        StopDamageBarCoroutine();

        if (!tookDamage)
        {
            hpbarFill.fillAmount = targetFillAmount;

            if (damageBarFill != null)
            {
                damageBarFill.fillAmount = targetFillAmount;
            }

            return;
        }

        if (damageBarFill != null)
        {
            damageBarFill.fillAmount = Mathf.Max(damageBarFill.fillAmount, previousFillAmount);
        }

        hpbarFill.fillAmount = targetFillAmount;

        if (damageBarFill == null) return;

        if (!gameObject.activeInHierarchy)
        {
            damageBarFill.fillAmount = targetFillAmount;
            return;
        }

        damageBarCoroutine = StartCoroutine(AnimateDamageBar(targetFillAmount));
    }

    private void SetHpImmediately(int currentHp, int maxHp)
    {
        StopDamageBarCoroutine();

        float fillAmount = CalculateFillAmount(currentHp, maxHp);

        if (hpbarFill != null) 
        {
            hpbarFill.fillAmount = fillAmount;
        }

        if (damageBarFill != null)
        {
            damageBarFill.fillAmount = fillAmount;
        }
    }

    private IEnumerator AnimateDamageBar(float targetFillAmount)
    {
        if (damageHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(damageHoldDuration);
        }

        float startFillAmount = damageBarFill.fillAmount;
        float elapsedTime = 0f;

        if (damageShrinkDuration <= 0f)
        {
            damageBarFill.fillAmount = targetFillAmount;

            damageBarCoroutine = null;
            yield break;
        }

        while (elapsedTime < damageShrinkDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / damageShrinkDuration);
            damageBarFill.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, progress);
            yield return null;
        }

        damageBarFill.fillAmount = targetFillAmount;
        damageBarCoroutine = null;
    }

    private float CalculateFillAmount(int currenHp, int maxHp)
    {
        if (maxHp <= 0) return 0f;

        return Mathf.Clamp01((float)currenHp / maxHp);
    }

    private void StopDamageBarCoroutine()
    {
        if (damageBarCoroutine == null) return;

        StopCoroutine(damageBarCoroutine);
        damageBarCoroutine = null;
    }

    public void Hide()
    {
        StopDamageBarCoroutine();
        UnbindCurrentBoss();

        if (bossPanel != null) bossPanel.SetActive(false);
    }

    public void UnbindCurrentBoss()
    {
        if (currentBossStat == null) return;

        currentBossStat.OnHpChanged -= HandleHpChanged;
        currentBossStat.OnDied -= Hide;

        currentBossStat = null;
    }

    private void OnDestroy()
    {
        UnbindCurrentBoss();

        if (instance == this)
        {
            instance = null;
        }
    }
}
