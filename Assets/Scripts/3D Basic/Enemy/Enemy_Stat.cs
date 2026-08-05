using System;
using System.Collections;
using UnityEngine;

public enum AttackType
{
    None,
    Normal,
    Knockback,
    Skill
}

public class Enemy_Stat : MonoBehaviour
{
    [Header("적 표시 정보")]
    public string EnemyName;

    [Tooltip("수동레벨")]
    [Min(1)]
    public int EnemyLevel = 1;

    [Header("몬스터 레벨 설정")]
    [Tooltip("현재 활성 캐릭터의 돌파 단계 상한을 몬스터 레벨로 사용")]
    [SerializeField] private bool useCharacterLevelCap = true;
    [Tooltip("플레이어 레벨을 찾지 못했을 때 사용할 레벨")]
    [SerializeField, Min(1)] private int fallbackEnemyLevel = 20;
    [Tooltip("몬스터 최대 레벨")]
    [SerializeField, Min(1)] private int maxEnemyLevel = 100;

    [Header("1레벨 기준 기본 스탯")]
    [Tooltip("이 몬스터의 1레벨 최대 HP")]
    [SerializeField, Min(1f)] private float level1MaxHP = 80f;
    [Tooltip("이 몬스터의 1레벨 공격력")]
    [SerializeField, Min(0f)] private float level1Attack = 10f;
    [Tooltip("이 몬스터의 1레벨 방어력")]
    [SerializeField, Min(0f)] private float level1Defense = 5f;

    [Header("레벨당 성장")]
    [Tooltip("0.12는 레벨마다 1레벨 HP의 12%씩 증가")]
    [SerializeField, Min(0f)] private float hpGrowthPerLevel = 0.12f;
    [Tooltip("0.08은 레벨마다 1레벨 공격력의 8%씩 증가")]
    [SerializeField, Min(0f)] private float attackGrowthPerLevel = 0.08f;
    [Tooltip("레벨마다 고정으로 더해지는 방어력")]
    [SerializeField, Min(0f)] private float defenseGrowthPerLevel = 2f;
    
    [Header("체력바")]
    [Tooltip("머리 위에 띄울지 여부")]
    [SerializeField] private bool useWorldHpBar = true;

    public event Action<int, int> OnHpChanged;
    public event Action OnDied;
    
    public float[] stats = new float[(int)STAT.STAT_COUNT];

    public void SetStat(STAT type, float value) { stats[(int)type] = value; }
    public float GetStat(STAT type) { return stats[(int)type]; }

    public int maxHP { get { return (int)GetStat(STAT.HP); } }
    public int attackPower { get { return (int)GetStat(STAT.Attack); } }
    public int defensePower { get { return (int)GetStat(STAT.Defense); } }

    public int currentHP = 80;
    public MonsterHpBar hpBar;
    private Canvas myCanvas;

    public Vector3 damageTextOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private float damageTextCameraForwardOffset = 0.45f;
    [SerializeField] private bool useColliderDamageTextPosition = true;

    private EnemyBase enemyBase;
    private Animator animator;
    private Enemy_AnimationEvent enemyAnimation;
    private Collider enemyCollider;

    private IEnumerator Start()
    {
        myCanvas = GetComponentInChildren<Canvas>(true);
        enemyBase = GetComponent<EnemyBase>();
        animator = GetComponentInChildren<Animator>();
        enemyAnimation = GetComponentInChildren<Enemy_AnimationEvent>();
        enemyCollider = GetComponent<Collider>();

        yield return null;

        ApplyEnemyLevelFromCharacterCap();
        RecalculateStatsByLevel();

        currentHP = maxHP;

        if (useWorldHpBar)
        {
            hpBar = GetComponentInChildren<MonsterHpBar>();

            if (hpBar != null)
            {
                hpBar.Setup(this);
            }
        }
        else
        {
            hpBar = null;
        }

        Debug.Log($"[{EnemyName}] Lv.{EnemyLevel} 스탯 적용 완료 - " + $"HP: {maxHP}, ATK: {attackPower}, DEF: {defensePower}");
    }

    public void RecalculateStatsByLevel()
    {
        EnemyLevel = Mathf.Clamp(EnemyLevel, 1, maxEnemyLevel);

        int levelIncreaseCount = EnemyLevel - 1;

        float calculatedHP = level1MaxHP * (1f + hpGrowthPerLevel * levelIncreaseCount);

        float calculatedAttack = level1Attack * (1f + attackGrowthPerLevel * levelIncreaseCount);

        float calculatedDefense = level1Defense + defenseGrowthPerLevel * levelIncreaseCount;

        SetStat(STAT.HP, Mathf.Max(Mathf.Round(calculatedHP), 1f));
        SetStat(STAT.Attack, Mathf.Max(Mathf.Round(calculatedAttack), 0f));
        SetStat(STAT.Defense, Mathf.Max(Mathf.Round(calculatedDefense), 0f));
    }

    public void SetEnemyLevel(int newLevel, bool refillHP = true)
    {
        EnemyLevel = Mathf.Clamp(newLevel, 1, maxEnemyLevel);

        RecalculateStatsByLevel();

        if (refillHP)
        {
            currentHP = maxHP;
        }
        else
        {
            currentHP = Mathf.Min(currentHP, maxHP);
        }

        OnHpChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int damage, AttackType type)
    {
        TakeDamage(damage, type, false);
    }

    public void TakeDamage(int damage, AttackType type, bool isPerfectEvadeBonus)
    {
        if (enemyBase == null || enemyBase.isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        OnHpChanged?.Invoke(currentHP, maxHP);
        
        if (DamageTextSpawner.instance != null)
        {
            Quaternion textRotation = Camera.main != null ? Camera.main.transform.rotation : Quaternion.identity;
            Vector3 spawnPosition = GetDamageTextSpawnPosition();

            DamageTextSpawner.instance.SpawnDamageText(damage, spawnPosition, textRotation, myCanvas, isPerfectEvadeBonus, Color.white);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            enemyBase.OnDamageTaken(player.transform);
        }

        if (currentHP <= 0)
        {
            OnDied?.Invoke();
            enemyBase.Dead();
            return;
        }

        switch (type)
        {
            case AttackType.Normal:
                enemyBase.EnterStunState(1.5f);
                break;
            case AttackType.Knockback:
                if (enemyBase.EnterStunState(1.5f))
                {
                    enemyBase.ApplyKnockback();
                }
                break;
            case AttackType.Skill:
                enemyBase.EnterStunState(1.5f, true);
                break;
        }
    }

    private Vector3 GetDamageTextSpawnPosition()
    {
        Vector3 basePosition = transform.position + damageTextOffset;

        if (useColliderDamageTextPosition && enemyCollider != null)
        {
            Bounds bounds = enemyCollider.bounds;
            basePosition = bounds.center;
            basePosition.y = bounds.max.y + damageTextOffset.y;
        }

        if (Camera.main == null) return basePosition;

        Vector3 cameraSideDirection = Camera.main.transform.position - transform.position;
        cameraSideDirection.y = 0f;

        if (cameraSideDirection.sqrMagnitude < 0.001f)
            cameraSideDirection = -Camera.main.transform.forward;

        cameraSideDirection.Normalize();

        return basePosition + cameraSideDirection * damageTextCameraForwardOffset;
    }

    private void ApplyEnemyLevelFromCharacterCap()
    {
        if (!useCharacterLevelCap)
        {
            EnemyLevel = Mathf.Clamp(EnemyLevel, 1, maxEnemyLevel);

            return;
        }

        Character_Stat activeCharacterStat = null;

        if (BattleManager.Instance != null)
        {
            GameObject activeCharacter = BattleManager.Instance.GetActiveCharacter();

            if (activeCharacter != null)
            {
                activeCharacterStat = activeCharacter.GetComponent<Character_Stat>();

                if (activeCharacterStat == null)
                {
                    activeCharacterStat = activeCharacter.GetComponentInChildren<Character_Stat>(true);
                }
            }
        }

        if (activeCharacterStat == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                activeCharacterStat = playerObject.GetComponent<Character_Stat>();

                if (activeCharacterStat == null)
                {
                    activeCharacterStat = playerObject.GetComponentInChildren<Character_Stat>(true);
                }
            }
        }

        if (activeCharacterStat == null || activeCharacterStat.characterData == null || Character_Manager.Instance == null)
        {
            EnemyLevel = Mathf.Clamp(fallbackEnemyLevel, 1, maxEnemyLevel);

            Debug.LogWarning($"[{gameObject.name}] 활성 캐릭터의 돌파 상한을 찾지 못해 " + $"대체 레벨 Lv.{EnemyLevel}을 사용합니다.");

            return;
        }

        CharacterStatus playerStatus = Character_Manager.Instance.GetCharacterStatus(activeCharacterStat.characterData.characterID, activeCharacterStat.characterData);

        int characterLevelCap = Character_Manager.Instance.GetCurrentLevelCap(playerStatus);

        EnemyLevel = Mathf.Clamp(characterLevelCap, 1, maxEnemyLevel);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxEnemyLevel = Mathf.Max(maxEnemyLevel, 1);

        EnemyLevel = Mathf.Clamp(EnemyLevel, 1, maxEnemyLevel);

        if (stats == null || stats.Length != (int)STAT.STAT_COUNT)
        {
            Array.Resize(ref stats, (int)STAT.STAT_COUNT);
        }

        RecalculateStatsByLevel();
    }
#endif
}
