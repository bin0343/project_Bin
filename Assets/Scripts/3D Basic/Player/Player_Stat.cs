using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Stat : MonoBehaviour
{
    public static Player_Stat globalInstance;

    public float[] baseStats = new float[(int)STAT.STAT_COUNT];
    private float[] equipmentStats = new float[(int)STAT.STAT_COUNT];

    public void SetStat(STAT type, float value)
    {
        baseStats[(int)type] = value;
    }

    public float GetStat(STAT type)
    {
        return baseStats[(int)type];
    }
    public void AddEquipmentStat(STAT type, float value)
    {
        equipmentStats[(int)type] += value;
    }
    public void RemoveEquipmentStat(STAT type, float value)
    {
        equipmentStats[(int)type] -= value;
    }

    public int maxHP { get { return (int)(GetStat(STAT.HP) + equipmentStats[(int)STAT.HP]); } }
    public int maxMP { get { return (int)(GetStat(STAT.MP) + equipmentStats[(int)STAT.MP]); } }
    public int attackPower { get { return (int)(GetStat(STAT.Attack) + equipmentStats[(int)STAT.Attack]); } }
    public int defensePower { get { return (int)(GetStat(STAT.Defense) + equipmentStats[(int)STAT.Defense]); } }

    public int level = 1;
    public int exp = 0;
    public int levelUpExp = 100;
    public int currentHP = 100;
    public int currentMP = 100;
    public int gold;
    public int currentAP = 100;
    public int maxAP = 100;

    [Header("스태미나")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRegenRate = 3f;    //초당 스태미나 회복량
    public float rollStaminaCost = 20f;     //구르기 스태미나

    private Canvas myCanvas;

    public Vector3 damageTextOffset = new Vector3(0, 2.5f, 0);

    private Player_Action action;
    public bool isGlobalData = false;

    private void Awake()
    {
        // 'Player_Inventory'와 같은 오브젝트에 있다면 -> Global Data
        if (GetComponent<Player_Inventory>() != null)
        {
            // 이미 전역 데이터(globalInstance)가 살아있다면
            // 씬 이동으로 인해 생긴 '임시 중복 오브젝트'이므로 연결하지 않고 무시
            if (globalInstance != null && globalInstance != this)
            {
                return;
            }

            isGlobalData = true;
            globalInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            isGlobalData = false;
        }
    }

    void Start()
    {
        //만약 나는 캐릭터(Local)인데, Global 데이터가 있다면? -> 동기화
        if (!isGlobalData && globalInstance != null)
        {
            // Global에서 데이터 가져오기 (로드)
            this.level = globalInstance.level;
            this.gold = globalInstance.gold;
            this.exp = globalInstance.exp;
            this.baseStats = (float[])globalInstance.baseStats.Clone();

            currentHP = maxHP;
            currentMP = maxMP;

            Debug.Log("캐릭터가 생성되어 Global 데이터를 불러왔습니다.");
        }

        myCanvas = GetComponentInChildren<Canvas>(true);
        action = GetComponent<Player_Action>();

        if (!isGlobalData && UI_Manager.instance != null)
        {
            UI_Manager.instance.UpdatePlayerStatus(this);
        }
    }

    private void Update()
    {
        if (currentStamina < maxStamina)
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
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            return true;
        }
        return false; // 스태미나 부족
    }

    public void TakeDamage(int damage)
    {
        if (action.IsInvincible) return;
        if (isGlobalData) return;
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);
        action?.OnDamageTaken();
        Debug.Log("플레이어가 피해를 입음. 남은 체력: " + currentHP);

        if (DamageTextSpawner.instance != null)
        {
            Quaternion textRotation = Camera.main.transform.rotation;
            Vector3 spawnPosition = transform.position + damageTextOffset;

            DamageTextSpawner.instance.SpawnDamageText(damage, spawnPosition, textRotation, myCanvas);
        }

        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.UpdatePlayerStatus(this);
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        if (!isGlobalData && UI_Manager.instance != null)
        {
            UI_Manager.instance.UpdatePlayerStatus(this);
        }
    }

    public void RecoverMp(int amount)
    {
        currentMP = Mathf.Clamp(currentMP + amount, 0, maxMP);
        if (!isGlobalData && UI_Manager.instance != null)
        {
            UI_Manager.instance.UpdatePlayerStatus(this);
        }
    }

    public void GainExp(int amount)
    {
        exp += amount;
        while (exp >= levelUpExp)
        {
            exp -= levelUpExp;
            LevelUp();
        }

        // Global 동기화
        if (!isGlobalData && globalInstance != null)
        {
            globalInstance.exp = this.exp;
            globalInstance.level = this.level;
        }
    }

    public void GainGold(int amount)
    {
        gold += amount;

        // 내가 캐릭터라면, Global에도 반영해줘야 함
        if (!isGlobalData && globalInstance != null)
        {
            globalInstance.gold = this.gold;
        }
        else if (isGlobalData)
        {
            // 내가 Global이라면 그냥 저장
            // (필요하다면 PlayerPrefs 저장 로직 추가)
        }
        if (!isGlobalData && UI_Manager.instance != null)
        {
            UI_Manager.instance.UpdatePlayerStatus(this);
        }
    }

    private void LevelUp()
    {
        level++;
        levelUpExp *= 2;

        SetStat(STAT.HP, GetStat(STAT.HP) + 10);
        SetStat(STAT.MP, GetStat(STAT.MP) + 5);
        currentHP = maxHP;
        currentMP = maxMP;

        //StatPoints += 3;
    }

    public bool UseAP(int amount)
    {
        if (currentAP >= amount)
        {
            currentAP -= amount;
            // UI 갱신 호출
            return true; // 사용 성공
        }
        return false; // 행동력 부족
    }
    
    //저장된 데이터로 스탯 덮어씌우기
    public void LoadStatsFromSaveData(int lvl, int gld, int ex, float[] savedStats)
    {
        this.level = lvl;
        this.gold = gld;
        this.exp = ex;

        // 저장된 스탯 배열이 있고, 개수가 맞으면 복사
        if (savedStats != null && savedStats.Length == baseStats.Length)
        {
            // 배열 값을 하나씩 복사 (참조가 아니라 값 복사)
            for (int i = 0; i < baseStats.Length; i++)
            {
                baseStats[i] = savedStats[i];
            }
        }

        // UI가 있다면 갱신
        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.UpdatePlayerStatus(this);
        }
    }
}
