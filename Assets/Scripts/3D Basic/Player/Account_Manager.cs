using System;
using UnityEngine;

public class Account_Manager : MonoBehaviour
{
    public static Account_Manager Instance;

    [Header("계정 레벨 및 재화")]
    public int accountLevel = 1;
    public int accountExp = 0;
    public int levelUpExp = 100;
    public int gold = 0;

    [Header("행동력")]
    public int currentAP = 240;
    public int maxAP = 240;

    [Header("행동력 자동 회복")]
    [SerializeField, Min(1)]
    private int apRecoverySecondsPerPoint = 360;

    [SerializeField, Min(0.1f)]
    private float apRecoveryCheckInterval = 1f;

    private float apRecoveryCheckTimer;
    private long lastAPUpdateUtcTicks;

    [Header("파티 공용 스태미나 (대시/구르기)")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRegenRate = 10f;
    public float rollStaminaCost = 15f;
    [HideInInspector] public bool isExhausted = false;
    private float exhaustionTimer = 0f;
    private const float exhaustionDuration = 5f;

    public event Action<int, int> OnAPChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isExhausted)
        {
            exhaustionTimer += Time.deltaTime;
            if (exhaustionTimer >= exhaustionDuration)
            {
                //2초 지나면 탈진 해제, 회복 시작
                isExhausted = false;
                exhaustionTimer = 0f;
            }
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }

        apRecoveryCheckTimer += Time.unscaledDeltaTime;

        if (apRecoveryCheckTimer >= apRecoveryCheckInterval)
        {
            apRecoveryCheckTimer = 0f;

            ApplyAPRecovery();
        }
    }

    public bool TryUseStamina(float amount)
    {
        if (isExhausted || currentStamina <= 0) return false;

        currentStamina -= amount;

        if (currentStamina <= 0)
        {
            currentStamina = 0;
            isExhausted = true;
            exhaustionTimer = 0f;
        }
        return true; // 구르기 성공
    }

    public void GainGold(int amount)
    {
        gold += amount;
        GameDataManager.Instance?.RequestAutoSave();
        Debug.Log($"골드 획득: {amount} / 현재 골드: {gold}");
        // TODO: 상단 재화 UI 갱신
    }

    public bool UseAP(int amount)
    {
        if (amount <= 0) return true;

        ApplyAPRecovery();

        if (currentAP < amount) return false;

        bool wasFull = currentAP >= maxAP;

        currentAP -= amount;

        if (wasFull)
        {
            lastAPUpdateUtcTicks = DateTime.UtcNow.Ticks;
        }

        NotifyAPChanged();
        GameDataManager.Instance?.RequestAutoSave();

        Debug.Log($"[행동력] {amount} 소모 / " + $"현재 {currentAP}/{maxAP}");

        return true;
    }

    private void ApplyAPRecovery()
    {
        long currentUtcTicks = DateTime.UtcNow.Ticks;

        if (currentAP >= maxAP)
        {
            currentAP = maxAP;

            lastAPUpdateUtcTicks = currentUtcTicks;

            return;
        }

        long elapsedTicks = currentUtcTicks - lastAPUpdateUtcTicks;

        if (elapsedTicks <= 0) return;

        double elapsedSeconds = elapsedTicks / (double)TimeSpan.TicksPerSecond;

        int recoveredAmount = Mathf.FloorToInt((float)(elapsedSeconds / apRecoverySecondsPerPoint));

        if (recoveredAmount <= 0) return;

        int previousAP = currentAP;

        currentAP = Mathf.Min(currentAP + recoveredAmount, maxAP);

        if (currentAP >= maxAP)
        {
            lastAPUpdateUtcTicks = currentUtcTicks;
        }
        else
        {
            long consumedTicks = (long)recoveredAmount * apRecoverySecondsPerPoint * TimeSpan.TicksPerSecond;

            lastAPUpdateUtcTicks += consumedTicks;
        }

        if (currentAP != previousAP)
        {
            Debug.Log($"[행동력] {recoveredAmount} 회복 / " + $"현재 {currentAP}/{maxAP}");

            NotifyAPChanged();
            GameDataManager.Instance?.RequestAutoSave();
        }
    }
    
    public void GainAccountExp(int amount)
    {
        accountExp += amount;
        while (accountExp >= levelUpExp)
        {
            accountExp -= levelUpExp;
            accountLevel++;
            levelUpExp *= 2; // 임시 공식
            GameDataManager.Instance?.RequestAutoSave();
            Debug.Log($"계정 레벨업! 현재 레벨: {accountLevel}");
        }
    }

    private void NotifyAPChanged()
    {
        OnAPChanged?.Invoke(currentAP, maxAP);
    }

    //첫 플레이 유저
    public void InitializeNewAccount()
    {
        accountLevel = 1;
        accountExp = 0;
        gold = 0;

        currentAP = maxAP;
        lastAPUpdateUtcTicks = DateTime.UtcNow.Ticks;

        currentStamina = maxStamina;
        isExhausted = false;
        exhaustionTimer = 0f;

        NotifyAPChanged();

        Debug.Log("[Account] 신규 계정 초기 데이터 생성");
    }

    #region Save & Load

    public AccountSaveData GetSaveData()
    {
        ApplyAPRecovery();

        AccountSaveData data = new AccountSaveData();

        data.accountLevel = accountLevel;
        data.accountExp = accountExp;
        data.gold = gold;

        data.currentAP = currentAP;
        data.lastAPUpdateUtcTicks = lastAPUpdateUtcTicks;

        return data;
    }

    public void LoadSaveData(AccountSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[Account] 불러올 계정 데이터가 없습니다.");

            return;
        }

        accountLevel = Mathf.Max(1, data.accountLevel);
        accountExp = Mathf.Max(0, data.accountExp);
        gold = Mathf.Max(0, data.gold);

        currentAP = Mathf.Clamp(data.currentAP, 0, maxAP);

        lastAPUpdateUtcTicks = data.lastAPUpdateUtcTicks;

        if (lastAPUpdateUtcTicks <= 0)
        {
            lastAPUpdateUtcTicks = DateTime.UtcNow.Ticks;
        }

        ApplyAPRecovery();
        NotifyAPChanged();

        Debug.Log($"[Account] 계정 데이터 로드 완료 / " + $"Lv.{accountLevel} / " + $"AP {currentAP}/{maxAP}");
    }

    #endregion

    private void OnApplicationPause(bool isPaused)
    {
        if (!isPaused)
        {
            ApplyAPRecovery();
        }
    }
}
