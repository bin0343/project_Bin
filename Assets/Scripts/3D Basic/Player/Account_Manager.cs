using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Account_Manager : MonoBehaviour
{
    public static Account_Manager instance;

    [Header("계정 레벨 및 재화")]
    public int accountLevel = 1;
    public int accountExp = 0;
    public int levelUpExp = 100;
    public int gold = 0;

    [Header("행동력")]
    public int currentAP = 240;
    public int maxAP = 240;

    [Header("파티 공용 스태미나 (대시/구르기)")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRegenRate = 10f;
    public float rollStaminaCost = 15f;
    [HideInInspector] public bool isExhausted = false;
    private float exhaustionTimer = 0f;
    private const float exhaustionDuration = 5f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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

            // UI 업데이트 함수가 있다면 여기서 호출 (예: UpdateStaminaUI();)
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
        Debug.Log($"골드 획득: {amount} / 현재 골드: {gold}");
        // TODO: 상단 재화 UI 갱신
    }

    public bool UseAP(int amount)
    {
        if (currentAP >= amount)
        {
            currentAP -= amount;
            return true; // 사용 성공
        }
        return false; // 행동력 부족
    }

    public void GainAccountExp(int amount)
    {
        accountExp += amount;
        while (accountExp >= levelUpExp)
        {
            accountExp -= levelUpExp;
            accountLevel++;
            levelUpExp *= 2; // 임시 공식
            Debug.Log($"계정 레벨업! 현재 레벨: {accountLevel}");
        }
    }
}
