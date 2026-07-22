using System;
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
    public int EnemyLevel = 1;

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
    public int ExpReward = 50;
    public MonsterHpBar hpBar;
    private Canvas myCanvas;

    public Vector3 damageTextOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private float damageTextCameraForwardOffset = 0.45f;
    [SerializeField] private bool useColliderDamageTextPosition = true;

    private EnemyBase enemyBase;
    private Animator animator;
    private Enemy_AnimationEvent enemyAnimation;
    private Collider enemyCollider;

    void Start()
    {
        myCanvas = GetComponentInChildren<Canvas>(true);
        enemyBase = GetComponent<EnemyBase>();
        animator = GetComponentInChildren<Animator>();
        enemyAnimation = GetComponentInChildren<Enemy_AnimationEvent>();
        enemyCollider = GetComponent<Collider>();

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
}
