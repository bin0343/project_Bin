using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    None,
    Normal,
    Knockback
}

public class Enemy_Stat : MonoBehaviour
{
    public string EnemyName;
    //public int MaxHP = 80;
    public int ExpReward = 50;
    

    //public int AttackPower = 8;
    //public int DefensePower = 3;

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

    private EnemyBase enemyBase;
    private Animator animator;

    private Enemy_AnimationEvent enemyAnimation;

    void Start()
    {
        hpBar = GetComponentInChildren<MonsterHpBar>();
        if (hpBar != null)
            hpBar.Setup(this);

        myCanvas = GetComponentInChildren<Canvas>(true);
        enemyBase = GetComponent<EnemyBase>();
        animator = GetComponentInChildren<Animator>();
        enemyAnimation = GetComponentInChildren<Enemy_AnimationEvent>();
        currentHP = maxHP;
    }

    public void TakeDamage(int damage, AttackType type)
    {
        if (enemyBase.isDead) return;

        int prevHP = currentHP;
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);
        
        if (DamageTextSpawner.instance != null)
        {
            Quaternion textRotation = Camera.main.transform.rotation;
            Vector3 spawnPosition = transform.position + damageTextOffset;

            DamageTextSpawner.instance.SpawnDamageText(damage, spawnPosition, textRotation, myCanvas);
        }

        if (hpBar != null)
            hpBar.UpdateHpBar();

        if (currentHP <= 0)
        {
            enemyBase.Dead(); // EnemyBase의 Dead 함수를 public으로 변경해야 합니다.
        }
    }
}
