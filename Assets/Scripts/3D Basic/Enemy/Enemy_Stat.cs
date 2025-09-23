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
    public int MaxHP = 80;
    public int CurrentHP = 80;

    public int AttackPower = 8;
    public int DefensePower = 3;

    public int ExpReward = 50;
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
    }

    public void TakeDamage(int damage, AttackType type)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(CurrentHP, 0);

        if (CurrentHP > 0 && damage >= 1)
        {
            switch (type)
            {
                case AttackType.None:
                    break;
                case AttackType.Normal:
                    Stun();
                    break;
                case AttackType.Knockback:
                    Stun();
                    StartCoroutine(enemyBase.ApplyKnockback());
                    break;
            }
        }
        
        if (DamageTextSpawner.instance != null)
        {
            Quaternion textRotation = Camera.main.transform.rotation;
            Vector3 spawnPosition = transform.position + damageTextOffset;

            DamageTextSpawner.instance.SpawnDamageText(damage, spawnPosition, textRotation, myCanvas);
        }

        if (hpBar != null)
            hpBar.UpdateHpBar();
    }

    private void Stun()
    {
        if (enemyBase.slashTrail != null)
        {
            enemyBase.slashTrail.emitting = false;
        }
        animator.SetTrigger("IsStun");
        
    }
}
